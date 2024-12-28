using BlazorWith3d.Babylon;
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
    
    private readonly List<byte[]> _unsentMessages = new();

    protected virtual string InitializeMethodName => "InitializeApp";
    
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    protected override JsMessageReceiverProxy CreateReceiverProxy()
    {
        return new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }
        await base.OnAfterRenderAsync(firstRender);

        foreach (var msg in _unsentMessages)
        {
            await SendMessage(msg);
        }

        _unsentMessages.Clear();

    }


    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module, DotNetObjectReference<JsMessageReceiverProxy> messageReceiverProxyReference)
    { 
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived) );
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
            _unsentMessages.Add(messageBytes);
            return;
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
    
    protected class BinaryApiJsMessageReceiverProxy(Action<byte[]> onMessageBytesReceived):JsMessageReceiverProxy
    {
        [JSInvokable]
        public void OnMessageBytesReceived(byte[] msg)
        {
            onMessageBytesReceived(msg);
        }
    }
}