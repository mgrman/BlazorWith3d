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
    public class BlazorWebSocketRelay : IAsyncDisposable, IBinaryApi
    {
        private readonly string _url;

        private ClientWebSocket _ws;

        private readonly CancellationTokenSource _cts;
        private readonly List<IBufferWriterWithArraySegment<byte>> _unsentMessages= new ();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }
        
        public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;


        public BlazorWebSocketRelay(string url)
        {
            _url = url;
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await ConnectAsync();
                    await Task.Delay(1000, _cts.Token);
                }
            });
        }

        private async Task ConnectAsync()
        {
            if (_ws != null)
            {
                if (_ws.State == WebSocketState.Open)
                {
                    return;
                }
                else
                {
                    _ws.Dispose();
                }
            }
            Debug.Log($"Connecting to {_url}");

            _ws = new ClientWebSocket();

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await _ws.ConnectAsync(new Uri(_url), _cts.Token);
                Debug.Log($"Connected to {_url}");
            }
            catch (ObjectDisposedException ex)
            {
                return;
            }
            catch (WebSocketException ex)
            {
                return;
            }

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            await Task.Factory.StartNew(ReceiveLoop, _cts.Token, TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            foreach (var unsentMessage in _unsentMessages)
            {
                
              await  SendMessageInner(0,unsentMessage.WrittenArray);
                unsentMessage.Dispose();
            }
            _unsentMessages.Clear();
        }

        private async Task ReceiveLoop()
        {
            var loopToken = _cts.Token;
            
            var buffer =new ArraySegment<byte>( new byte[1024 * 4]);
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    var receiveResult = await _ws.ReceiveAsync(buffer, _cts.Token);
                    if (!receiveResult.EndOfMessage)
                    {
                        throw new InvalidOperationException("Did not receive full message!");
                    }

                    await ResponseReceived(buffer.Slice(0, receiveResult.Count).ToArray());
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            if (_ws?.State!= WebSocketState.Open)
            {
                _unsentMessages.Add(bytes);
                return ;
            }

            
            Debug.Log($"SendMessage at {Time.realtimeSinceStartup}");
            
            await SendMessageInner(0,bytes.WrittenArray);
            
            bytes.Dispose();
            
            return ;
        }

        public async ValueTask UpdateScreen(byte[] bytes)
        {
            if (_ws?.State!= WebSocketState.Open)
            {
                return ;
            }
            
            Debug.LogWarning($"UpdateScreen at {Time.realtimeSinceStartup}");

            await SendMessageInner(1,bytes);
            return ;
        }

        private async Task SendMessageInner(byte prefix, ArraySegment<byte> bytes)
        {
            await _semaphore.WaitAsync();
           await _ws.SendAsync(new []{prefix}, WebSocketMessageType.Binary, false, _cts.Token);
           await _ws.SendAsync(bytes, WebSocketMessageType.Binary, true, _cts.Token);
           _semaphore.Release();
        }

        private async ValueTask ResponseReceived(byte[] data)
        {
            await Awaitable.MainThreadAsync();
            try
            {
                Debug.LogError($"MessageReceived at {Time.realtimeSinceStartup}");
                await (MainMessageHandler?.Invoke(data) ?? new ValueTask());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_ws is null)
            {
                return;
            }

            _cts.Cancel();

            if (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }

            _ws.Dispose();
            _ws = null;
            _cts.Dispose();
        }
    }
}
#endif