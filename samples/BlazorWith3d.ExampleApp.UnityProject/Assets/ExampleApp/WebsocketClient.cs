using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace ExampleApp
{
  
    public class WsClient : IDisposable, IBlazorApi {

        public int ReceiveBufferSize { get; set; } = 8192;

        public WsClient()
        {
            
            Application.quitting += this.Dispose;
        }
        
        public async Task ConnectAsync(string url) {
            if (WS != null) {
                if (WS.State == WebSocketState.Open) return;
                else WS.Dispose();
            }
            WS = new ClientWebSocket();
            if (CTS != null) CTS.Dispose();
            CTS = new CancellationTokenSource();
            await WS.ConnectAsync(new Uri(url), CTS.Token);
            await Task.Factory.StartNew(ReceiveLoop, CTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task DisconnectAsync() {
            if (WS is null) return;
            // TODO: requests cleanup code, sub-protocol dependent.
            if (WS.State == WebSocketState.Open) {
                CTS.CancelAfter(TimeSpan.FromSeconds(2));
                await WS.CloseOutputAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                await WS.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            WS.Dispose();
            WS = null;
            CTS.Dispose();
            CTS = null;
        }

        private async Task ReceiveLoop() {
            var loopToken = CTS.Token;
            MemoryStream outputStream = null;
            WebSocketReceiveResult receiveResult = null;
            var buffer = new byte[ReceiveBufferSize];
            try
            {
                while (!loopToken.IsCancellationRequested)
                {
                    outputStream = new MemoryStream(ReceiveBufferSize);
                    do
                    {
                        receiveResult = await WS.ReceiveAsync(buffer, CTS.Token);
                        if (receiveResult.MessageType != WebSocketMessageType.Close)
                            outputStream.Write(buffer, 0, receiveResult.Count);
                    } while (!receiveResult.EndOfMessage);

                    if (receiveResult.MessageType == WebSocketMessageType.Close) break;
                    outputStream.Position = 0;
                    ResponseReceived(outputStream);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally {
                outputStream?.Dispose();
            }
        }

        public void SendMessageToBlazor(byte[] bytes)
        {
            WS.SendAsync(bytes, WebSocketMessageType.Binary, true, CTS.Token);
        }
        
        private async void ResponseReceived(Stream inputStream) {
            // TODO: handle deserializing responses and matching them to the requests.
            // IMPORTANT: DON'T FORGET TO DISPOSE THE inputStream!

            using var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            inputStream.Dispose();

            await Awaitable.MainThreadAsync();
            OnMessageFromBlazor?.Invoke(ms.ToArray());
        }

        public void Dispose() => DisconnectAsync().Wait();

        private ClientWebSocket WS;
        private CancellationTokenSource CTS;

        public Action<byte[]> OnMessageFromBlazor { get; set; }
    }
}