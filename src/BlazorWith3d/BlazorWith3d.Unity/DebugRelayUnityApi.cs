
using System.Net.WebSockets;
using BlazorWith3d.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Unity;

public class DebugRelayUnityApi:IBinaryApi
{
    private readonly ILogger<DebugRelayUnityApi> _logger;
    private WebSocket? _webSocket;
    private readonly List<byte[]> _unsentMessages= new();
    private readonly BackgroundMessageBuffer _unhandledMessages= new();
    private Func<Action,Task>?  _handleThread;

    public Action<byte[]>? MainMessageHandler { get => _unhandledMessages.MainMessageHandler; set => _unhandledMessages.MainMessageHandler = value; }

    public DebugRelayUnityApi(ILogger<DebugRelayUnityApi> logger)
    {
        _logger = logger;
    }
    
    public void HandleThread(Func<Action,Task> action)
    {
        _unhandledMessages.HandleThread = action;
    }
    
    public async ValueTask SendMessage(byte[] bytes)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            _unsentMessages.Add(bytes);
            return;
        }

        await SendMessageInner(bytes);
    }

    private async Task SendMessageInner(byte[] bytes)
    {
        await _webSocket!.SendAsync(bytes,
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);
    }

    public async Task HandleWebSocket(WebSocket webSocket)
    {
        _webSocket = webSocket;
        
        foreach (var unsentMessage in _unsentMessages)
        {
            await SendMessageInner(unsentMessage);
        }
        _unsentMessages.Clear();
        
        
        var buffer =new ArraySegment<byte>( new byte[1024 * 4]);
        while (webSocket.State == WebSocketState.Open)
        {

            try
            {
                var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                var msg = buffer.Slice(0, receiveResult.Count).ToArray();
                _unhandledMessages.InvokeMessageAsync(msg);
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex,"HandleWebSocket OnMessageFromUnity error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"HandleWebSocket OnMessageFromUnity error");
            }
        }

        
    }
}