#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using BlazorWith3d.Shared;

using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class BlazorWebSocketRelay : IAsyncDisposable
    {
        private readonly string _url;
        
        private readonly Func<BinaryApiForSocket?, ValueTask> _handler;

        private BinaryApiForSocket? _api;
        private readonly CancellationTokenSource _cts;


        public bool IsConnected => _api!=null;

        public BlazorWebSocketRelay(string url, Func<BinaryApiForSocket?, ValueTask> handler)
        {
            _url = url;
            _handler = handler;
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await ConnectAsync();
                    await Task.Delay(1000, _cts.Token);
                }
            }, _cts.Token);
        }

        private async Task ConnectAsync()
        {
            if (_api != null)
            {
                await _api.DisposeAsync();
                _api = null;
                await _handler.Invoke(null);
            }


            Debug.Log($"Connecting to {_url}");

            var ws = new ClientWebSocket();

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await ws.ConnectAsync(new Uri(_url), _cts.Token);
                Debug.Log($"Connected to {_url}");
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (WebSocketException)
            {
                return;
            }

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            var api= new BinaryApiForSocket(ws);
            _api = api;
            await _handler.Invoke(api);

            await api.HandleWebSocket();

            _api = null;
            await _handler.Invoke(null);
            await api.DisposeAsync();
        }

        public async ValueTask UpdateScreen(byte[] bytes)
        {
            if (_api==null)
            {
                return;
            }
            await _api.UpdateScreen( bytes);
        }

        public async ValueTask DisposeAsync()
        {
            if (_api != null)
            {
                await _api.DisposeAsync();
            }
             
            _cts.Cancel();
        }

    }


    public class BinaryApiForSocket : IBinaryMessageApi, IAsyncDisposable
    {
        private readonly WebSocket _webSocket;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _cts;

        public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }

        public BinaryApiForSocket(WebSocket webSocket)
        {
            _webSocket = webSocket;
            _cts= new CancellationTokenSource();
        }

        public async ValueTask HandleWebSocket()
        {
            int bufferIndex = 0;
            var buffer = new ArraySegment<byte>(new byte[1024 * 1024 * 4]);
            while (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var bufferToFill = buffer.Slice(bufferIndex);
                    var receiveResult = await _webSocket.ReceiveAsync(bufferToFill, _cts.Token).ConfigureAwait(false);
                    bufferIndex += receiveResult.Count;

                    if (receiveResult.EndOfMessage)
                    {
                        var msg = buffer.Slice(0, bufferIndex ).ToArray();
                        bufferIndex = 0;
                        await Awaitable.MainThreadAsync();
                        await (MainMessageHandler?.Invoke(msg) ?? new ValueTask());
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (WebSocketException)
                {
                    // Debug.LogWarning($"HandleWebSocket OnMessageFromUnity error {ex.Message}");
                    // if (ex.InnerException != null)
                    // {
                    //     Debug.LogWarning($"HandleWebSocket InnerException: {ex.InnerException.Message}");
                    // }
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"HandleWebSocket OnMessageFromUnity error {ex.Message}");
                    return;
                }
            }
        }

        public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            Debug.Log($"SendMessage at {Time.realtimeSinceStartup}");
            await SendMessageInner(0, bytes.WrittenArray);

            bytes.Dispose();
        }

        public async ValueTask UpdateScreen(byte[] bytes)
        {
            await SendMessageInner(1, bytes);
        }

        private async Task SendMessageInner(byte prefix, ArraySegment<byte> bytes)
        {
            await _semaphore.WaitAsync();
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(new[] { prefix }, WebSocketMessageType.Binary, false, _cts.Token);
                await _webSocket.SendAsync(bytes, WebSocketMessageType.Binary, true, _cts.Token);
            }

            _semaphore.Release();
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            _webSocket.Dispose();
        }
    }

}
#endif