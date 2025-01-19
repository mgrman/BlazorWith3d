
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


        int bufferIndex = 0;
        var buffer =new ArraySegment<byte>( new byte[1024 *1024 * 4]);
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var bufferToFill = buffer.Slice(bufferIndex);
                var receiveResult = await webSocket.ReceiveAsync(bufferToFill, CancellationToken.None);
                bufferIndex+=receiveResult.Count;

                if (receiveResult.EndOfMessage)
                {
                    var msgType = buffer[0];
                    var msg = buffer.Slice(1, bufferIndex-1).ToArray();
                    bufferIndex = 0;
                    switch (msgType)
                    {
                        case 0:
                            api.MainMessageHandler.Invoke(msg);
                            break;
                        case 1:
                            api.OnNewFrame(msg);
                            break;
                        default:
                            throw new InvalidOperationException();

                    }
                }
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

        public Func<ArraySegment<byte>,ValueTask>? MainMessageHandler { get; set; }
        public event Action<byte[]> NewFrame;

        public void OnNewFrame(byte[] image)
        {
            NewFrame?.Invoke(image);
        }

        public  ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException();
            }

            _webSocket.SendAsync(bytes.WrittenArray,
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None);
            
            bytes.Dispose();
            
            return ValueTask.CompletedTask;
        }
    }
}