#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class BlazorWebSocketRelay : IAsyncDisposable, IBlazorApi
    {
        private readonly string _url;

        private ClientWebSocket _ws;

        private readonly CancellationTokenSource _cts;
        private readonly List<byte[]> _unsentMessages= new ();


        public Action<byte[]> OnMessageFromBlazor { get; set; }


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
                
                SendMessageInner(unsentMessage);
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

                    ResponseReceived(buffer.Slice(0, receiveResult.Count).ToArray());
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

        public void SendMessageToBlazor(byte[] bytes)
        {
            if (_ws?.State!= WebSocketState.Open)
            {
                _unsentMessages.Add(bytes);
                return;
            }

            SendMessageInner(bytes);
        }

        private void SendMessageInner(byte[] bytes)
        {
            _ws.SendAsync(bytes, WebSocketMessageType.Binary, true, _cts.Token);
        }

        private async void ResponseReceived(byte[] data)
        {
            await Awaitable.MainThreadAsync();
            OnMessageFromBlazor?.Invoke(data);
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