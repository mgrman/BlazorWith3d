using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWith3d.JsApp;

public class BaseJsBinaryApiWithResponseRenderer:BaseJsRenderer, IBinaryApiWithResponse
{
    
    [Inject]
    public required ILogger<BaseJsBinaryApiWithResponseRenderer> Logger { get; set; }
    
    [Inject]
    public required  IJSRuntime JsRuntime{ get; set; }
        
        
    public override string JsAppPath => "";

    protected virtual string InitializeMethodName => "InitializeApp";
    
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    private DotNetObjectReference<BinaryApiJsMessageReceiverProxy>? _messageReceiverProxyReference;


    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module)
    { 
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived,OnMessageBytesWithResponseReceived));
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived),nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesWithResponse) );
    }

    public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }
    public Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler { get; set; }


    private void OnMessageBytesReceived(byte[] messageBytes)
    {
        MainMessageHandler.Invoke(messageBytes);
    }
    
    private async ValueTask<byte[]> OnMessageBytesWithResponseReceived(byte[] messageBytes)
    {
       var response= await MainMessageWithResponseHandler.Invoke(messageBytes);
       var responseByteArray= response.WrittenArray.ToArray();// ToArray() as JS interop only has fast path for byte[] type
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
            await _typescriptApp.InvokeVoidAsync("ProcessMessage", messageBytes.WrittenArray.ToArray()); // ToArray() as JS interop only has fast path for byte[] type
            messageBytes.Dispose();
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

    public async ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> bytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();
            var response = await _typescriptApp.InvokeAsync<byte[]>("ProcessMessageWithResponse", bytes.WrittenArray.ToArray());// ToArray() as JS interop only has fast path for byte[] type
            bytes.Dispose();
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error sending message");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override ValueTask DisposeAsync()
    {
        _messageReceiverProxyReference?.Dispose();
        return base.DisposeAsync();
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