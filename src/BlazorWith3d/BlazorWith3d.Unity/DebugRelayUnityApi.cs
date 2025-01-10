
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
                api.MainMessageHandler.Invoke(msg);
            }
            catch (WebSocketException ex)
            {
                ConnectedApi?.Invoke(null);
                _logger.LogWarning(ex,"HandleWebSocket OnMessageFromUnity error");
            }
            catch (Exception ex)
            {
                ConnectedApi?.Invoke(null);
                _logger.LogError(ex,"HandleWebSocket OnMessageFromUnity error");
            }
        }
    }


    public class BinaryApiForSocket : IBinaryApi
    {
        private readonly WebSocket _webSocket;

        public BinaryApiForSocket(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public Func<byte[],ValueTask>? MainMessageHandler { get; set; }

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