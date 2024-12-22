
using System.Net.WebSockets;
using BlazorWith3d.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Unity;

public class DebugRelayUnityApi
{
    private readonly ILogger<DebugRelayUnityApi> _logger;
    private WebSocket? _webSocket;

    public event Action<BinaryApiForSocket?> ConnectedApi;
    
    public DebugRelayUnityApi(ILogger<DebugRelayUnityApi> logger)
    {
        _logger = logger;
    }
    
    
    
    public async Task HandleWebSocket(WebSocket webSocket)
    {
        _webSocket = webSocket;

        var api = new BinaryApiForSocket(webSocket);
        ConnectedApi?.Invoke(api);
        
        
        
        var buffer =new ArraySegment<byte>( new byte[1024 * 4]);
        while (webSocket.State == WebSocketState.Open)
        {

            try
            {
                var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                var msg = buffer.Slice(0, receiveResult.Count).ToArray();
                api._unhandledMessages.InvokeMessage(msg);
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


    public class BinaryApiForSocket : IBinaryApi
    {
        private readonly WebSocket _webSocket;
        public readonly ReceiveMessageBuffer _unhandledMessages= new();

        public BinaryApiForSocket(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public Action<byte[]>? MainMessageHandler { get => _unhandledMessages.MainMessageHandler; set => _unhandledMessages.MainMessageHandler = value; }

        public async ValueTask SendMessage(byte[] bytes)
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException();
            }

            await _webSocket.SendAsync(bytes,
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None);
        }
    }
}