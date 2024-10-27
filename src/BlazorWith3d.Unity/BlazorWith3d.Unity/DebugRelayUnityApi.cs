
using System.Net.WebSockets;
using BlazorWith3d.Unity.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Unity;

public class DebugRelayUnityApi:IBinaryApi
{
    private readonly ILogger<DebugRelayUnityApi> _logger;
    private WebSocket? _webSocket;
    private readonly List<byte[]> _unsentMessages= new();
    private readonly List<byte[]> _unhandledMessages= new();
    private Func<Action,Task>  _handleThread;
    private Action<byte[]>? _onMessage;

    public event Action<byte[]>? OnMessage
    {
        add
        {
            _onMessage += value;

            if (_unhandledMessages.Any())
            {
                var unhandledMessagesCopy = _unhandledMessages.ToList();
                _unhandledMessages.Clear();
                Task.Run(async () =>
                {
                    foreach (var unhandledMessage in unhandledMessagesCopy)
                    {
                        await _handleThread(() => _onMessage?.Invoke(unhandledMessage));
                    }
                });
            }
        }
        remove => _onMessage -= value;
    }

    public DebugRelayUnityApi(ILogger<DebugRelayUnityApi> logger)
    {
        _logger = logger;
    }
    
    public void HandleThread(Func<Action,Task> action)
    {
        _handleThread = action;
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
                if (_onMessage == null)
                {
                    _unhandledMessages.Add(msg);
                }
                else
                {
                    await _handleThread(() => _onMessage?.Invoke(msg));
                }
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