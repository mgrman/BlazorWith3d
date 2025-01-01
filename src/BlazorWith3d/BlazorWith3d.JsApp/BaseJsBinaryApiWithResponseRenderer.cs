using BlazorWith3d.Babylon;
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

    protected override JsMessageReceiverProxy CreateReceiverProxy()
    {
        return new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived,OnMessageBytesWithResponseReceived);
    }


    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module, DotNetObjectReference<JsMessageReceiverProxy> messageReceiverProxyReference)
    { 
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived),nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesWithResponse) );
    }

    public Func<byte[], ValueTask>? MainMessageHandler { get; set; }
    public Func<byte[], ValueTask<byte[]>>? MainMessageWithResponseHandler { get; set; }


    private void OnMessageBytesReceived(byte[] messageBytes)
    {
        MainMessageHandler.Invoke(messageBytes);
    }
    private ValueTask<byte[]> OnMessageBytesWithResponseReceived(byte[] messageBytes)
    {
       return MainMessageWithResponseHandler.Invoke(messageBytes);
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

    public async ValueTask<byte[]> SendMessageWithResponse(byte[] bytes)
    {
        if (_typescriptApp == null)
        {
            throw new InvalidOperationException();
        }

        try
        {
            await _semaphore.WaitAsync();
            var response = await _typescriptApp.InvokeAsync<byte[]>("ProcessMessageWithResponse", bytes);
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

    protected class BinaryApiJsMessageReceiverProxy(Action<byte[]> onMessageBytesReceived,Func<byte[], ValueTask<byte[]>> onMessageBytesWithResponseReceived):JsMessageReceiverProxy
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