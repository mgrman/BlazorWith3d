using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWith3d.JsApp;

public class BaseJsBinaryApiRenderer:BaseJsRenderer, IBinaryApi
{
    
    [Inject]
    public required ILogger<BaseJsBinaryApiRenderer> Logger { get; set; }
    
    [Inject]
    public required  IJSRuntime JsRuntime{ get; set; }
        
        
    public override string JsAppPath => "";
    
    protected virtual string InitializeMethodName => "InitializeApp";
    
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private DotNetObjectReference<BinaryApiJsMessageReceiverProxy>? _messageReceiverProxyReference;

    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module)
    { 
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived));
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived) );
    }

    public Func<byte[], ValueTask>? MainMessageHandler { get; set; }

    private void OnMessageBytesReceived(byte[] messageBytes)
    {
        MainMessageHandler.Invoke(messageBytes);
    }
    
    public async ValueTask SendMessage(byte[] messageBytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();
            await _typescriptApp.InvokeVoidAsync("ProcessMessage", messageBytes);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message");
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

    public override ValueTask DisposeAsync()
    {
        _messageReceiverProxyReference?.Dispose();
        return base.DisposeAsync();
    }
}