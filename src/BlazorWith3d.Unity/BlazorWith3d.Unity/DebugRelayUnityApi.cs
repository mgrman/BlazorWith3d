using System.Net.WebSockets;
using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity;

public class DebugRelayUnityApi:IUnityApi
{
    private WebSocket webSocket;
    private List<byte[]> _unsentMessages;
    private Func<Action,Task>  _handleThread;

    public Action<byte[]>? OnMessageFromUnity { get; set; }

    public void HandleThread(Func<Action,Task> action)
    {
        _handleThread = action;
    }
    
    public async Task SendMessageToUnity(byte[] bytes)
    {
        if (webSocket == null)
        {
            _unsentMessages.Add(bytes);
            return;
        }

        await SendMessageInner(bytes);
    }

    private async Task SendMessageInner(byte[] bytes)
    {
        await webSocket.SendAsync(bytes,
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);
    }

    public async Task HandleWebSocket(WebSocket webSocket)
    {
        this.webSocket = webSocket;
        var buffer =new ArraySegment<byte>( new byte[1024 * 4]);
        while (webSocket.State == WebSocketState.Open)
        {
            var receiveResult = await webSocket.ReceiveAsync(buffer
                , CancellationToken.None);

            try
            {
                await _handleThread(()=> OnMessageFromUnity?.Invoke(buffer.Slice(0, receiveResult.Count).ToArray()));
            }
            catch (Exception ex)
            {
                
            }
        }

        foreach (var unsentMessage in _unsentMessages)
        {
            await SendMessageInner(unsentMessage);
        }
        
        _unsentMessages.Clear();
    }
}