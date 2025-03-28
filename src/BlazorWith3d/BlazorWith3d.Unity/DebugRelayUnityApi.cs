
using System.Net.WebSockets;
using BlazorWith3d.Shared;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Unity;

public class DebugRelayUnityApi
{
    private readonly ILogger<DebugRelayUnityApi> _logger;

    public Action<BinaryApiForSocket?>? ConnectedApi
    {
        get;
        set
        {
            _connectedApiCts?.Cancel();
            _connectedApiCts = null;

            if (value !=null && field != null)
            {
                throw new InvalidOperationException("Already connected");
            }

            field = value;
            if (value != null)
            {
                _connectedApiCts = new CancellationTokenSource();
            }
        }
    }

    private CancellationTokenSource? _connectedApiCts;
    
    public event Action<byte[]> NewFrame;

    public DebugRelayUnityApi(ILogger<DebugRelayUnityApi> logger)
    {
        _logger = logger;
    }

    public bool CanHandleWebSocket()
    {
        return _connectedApiCts != null;
    }

    public async Task HandleWebSocket(WebSocket webSocket)
    {
        if (_connectedApiCts == null)
        {
            //nobody is listening
            return;
        }
        var token=_connectedApiCts.Token;
        var apiEvent = ConnectedApi;
        if (apiEvent == null)
        {
            _logger.LogError("Event is null but Cts is not!");
            return;
        }

        var api = new BinaryApiForSocket(webSocket);
        apiEvent.Invoke(api);

        int bufferIndex = 0;
        var buffer =new ArraySegment<byte>( new byte[1024 *1024 * 4]);
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var bufferToFill = buffer.Slice(bufferIndex);
                var receiveResult = await webSocket.ReceiveAsync(bufferToFill, token);
                bufferIndex += receiveResult.Count;

                if (receiveResult.EndOfMessage)
                {
                    var msgType = buffer[0];
                    var msg = buffer.Slice(1, bufferIndex - 1).ToArray();
                    bufferIndex = 0;
                    switch (msgType)
                    {
                        case 0:
                            await (api.MainMessageHandler?.Invoke(msg) ?? ValueTask.CompletedTask);
                            break;
                        case 1:
                            NewFrame?.Invoke(msg);
                            break;
                        default:
                            throw new InvalidOperationException();

                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                apiEvent.Invoke(null);
            }
            catch (WebSocketException ex)
            {
                apiEvent.Invoke(null);
                _logger.LogWarning(ex,"HandleWebSocket OnMessageFromUnity error");
                if (ex.InnerException != null)
                {
                    _logger.LogWarning(ex.InnerException, "HandleWebSocket InnerException");
                }
            }
            catch (Exception ex)
            {
                apiEvent.Invoke(null);
                _logger.LogError(ex,"HandleWebSocket OnMessageFromUnity error");
            }
        }
    }


    public class BinaryApiForSocket : IBinaryMessageApi
    {
        private readonly WebSocket _webSocket;

        public BinaryApiForSocket(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        public Func<ArraySegment<byte>,ValueTask>? MainMessageHandler { get; set; }

        public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            if (_webSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException();
            }

           await _webSocket.SendAsync(bytes.WrittenArray,
                WebSocketMessageType.Binary,
                true,
                CancellationToken.None);
            
            bytes.Dispose();
            
        }
    }
}