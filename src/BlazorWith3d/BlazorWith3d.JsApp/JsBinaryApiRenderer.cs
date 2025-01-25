using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWith3d.JsApp;

public class JsBinaryApiRenderer: IBinaryApi, IInitializableJsBinaryApi
{
    
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger _logger;
    
    private readonly SemaphoreSlim _semaphore = new (1, 1);

    private DotNetObjectReference<BinaryApiJsMessageReceiverProxy>? _messageReceiverProxyReference;
    private IJSObjectReference? _typescriptApp;

    public JsBinaryApiRenderer(IJSRuntime jsRuntime, ILogger logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public  async Task InitializeJsApp(string jsPath, ElementReference container, string initMethod="InitializeApp", object? extraArg=null)
    { 
        var module= await _jsRuntime.LoadModuleAsync(jsPath);

        _messageReceiverProxyReference = DotNetObjectReference.Create(new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived));
        _typescriptApp= await module.InvokeAsync<IJSObjectReference>(initMethod, container,extraArg,_messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived) );
    }

    public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }

    private void OnMessageBytesReceived(byte[] messageBytes)
    {
        MainMessageHandler.Invoke(messageBytes);
    }
    
    public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> messageBytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();

            (byte[] array, int offset) data = (messageBytes.WrittenArray.ToArray(), 0);
            
            await _typescriptApp.InvokeVoidAsync("ProcessMessage", data.array, data.offset);// ToArray() as JS interop only has fast path for byte[] type
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    public class BinaryApiJsMessageReceiverProxy(Action<byte[]> onMessageBytesReceived)
    {
        [JSInvokable]
        public void OnMessageBytesReceived(byte[] msg)
        {
            onMessageBytesReceived(msg);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _messageReceiverProxyReference?.Dispose();
        _messageReceiverProxyReference = null;
        await _typescriptApp.TryInvokeVoidAsync(_logger, "Quit");
        await _typescriptApp.TryDisposeAsync();
        _typescriptApp = null;
    }
}