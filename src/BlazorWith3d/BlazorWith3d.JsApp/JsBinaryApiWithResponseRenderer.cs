using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.JsApp;

public class JsBinaryApiWithResponseRenderer: IJsBinaryApi
{
    private readonly SemaphoreSlim _semaphore = new (1, 1);

    private DotNetObjectReference<BinaryApiJsMessageReceiverProxy>? _messageReceiverProxyReference;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger _logger;
    private IJSObjectReference? _typescriptApp;

    public JsBinaryApiWithResponseRenderer(IJSRuntime jsRuntime, ILogger logger)
     {
         _jsRuntime = jsRuntime;
         _logger = logger;
     }

    public  async Task InitializeJsApp(string jsPath, ElementReference container, string initMethod="InitializeApp", object? extraArg=null )
    { 
        var module= await _jsRuntime.LoadModuleAsync(jsPath);
        
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived,OnMessageBytesWithResponseReceived));
        _typescriptApp=   await module.InvokeAsync<IJSObjectReference>(initMethod, container,extraArg,_messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived),nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesWithResponse) );
    }

    public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }
    
    public Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler { get; set; }

    private void OnMessageBytesReceived(byte[] messageBytes)
    {
        if (MainMessageHandler == null)
        {
            throw new InvalidCastException(
                $"Received a message {messageBytes.Length} without a MainMessageHandler being set!");
        }

        MainMessageHandler.Invoke(messageBytes);
    }

    private async ValueTask<byte[]> OnMessageBytesWithResponseReceived(byte[] messageBytes)
    {
        if (MainMessageWithResponseHandler == null)
        {
            throw new InvalidCastException(
                $"Received a message {messageBytes.Length} without a MainMessageWithResponseHandler being set!");
        }

        var response = await MainMessageWithResponseHandler.Invoke(messageBytes);
        var responseByteArray =
            response.WrittenArray.ToArray(); // ToArray() as JS interop only has fast path for byte[] type
        response.Dispose();
        return responseByteArray;
    }

    public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte>  messageBytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();
            
            (byte[] array, int offset, int count) data = GetArraysForInterop(messageBytes);
            await _typescriptApp.InvokeVoidAsync("ProcessMessage",data.array, data.offset, data.count); // ToArray() as JS interop only has fast path for byte[] type
            messageBytes.Dispose();
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

    protected virtual (byte[] array, int offset, int count) GetArraysForInterop(IBufferWriterWithArraySegment<byte> messageBytes)
    {
        return (messageBytes.WrittenArray.ToArray(), 0, messageBytes.WrittenArray.Count);
    }

    public async ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> messageBytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();
            (byte[] array, int offset, int count) data= GetArraysForInterop(messageBytes);
            var response = await _typescriptApp.InvokeAsync<byte[]>("ProcessMessageWithResponse", data.array, data.offset, data.count);// ToArray() as JS interop only has fast path for byte[] type
            messageBytes.Dispose();
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _messageReceiverProxyReference?.Dispose();
        _messageReceiverProxyReference = null;
        if (_typescriptApp != null)
        {
            await _typescriptApp.TryInvokeVoidAsync(_logger, "Quit");
            await _typescriptApp.TryDisposeAsync();
        }

        _typescriptApp = null;
    }

    protected class BinaryApiJsMessageReceiverProxy(Action<byte[]> onMessageBytesReceived,Func<byte[], ValueTask<byte[]>> onMessageBytesWithResponseReceived)
    {
        [JSInvokable]
        public void OnMessageBytesReceived(byte[] msg)
        {
            onMessageBytesReceived(msg);
        }
        [JSInvokable]
        public ValueTask<byte[]> OnMessageBytesWithResponse(byte[] msg)
        {
           return onMessageBytesWithResponseReceived(msg);
        }
    }
}