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


    [Parameter] 
    public bool CopyArrays { get; set; } = true;

    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module)
    { 
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BinaryApiJsMessageReceiverProxy(OnMessageBytesReceived));
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference,nameof(BinaryApiJsMessageReceiverProxy.OnMessageBytesReceived) );
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

            (byte[] array, int offset) data;
            if (CopyArrays)
            {
                data = (messageBytes.WrittenArray.ToArray(), 0);
            }
            else
            {
                data = (messageBytes.WrittenArray.Array!, messageBytes.WrittenArray.Offset);
            }
            
            await _typescriptApp.InvokeVoidAsync("ProcessMessage", data.array, data.offset);// ToArray() as JS interop only has fast path for byte[] type
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