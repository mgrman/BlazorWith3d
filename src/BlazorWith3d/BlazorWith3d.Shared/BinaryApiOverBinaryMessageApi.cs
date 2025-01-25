using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public class BinaryApiOverBinaryMessageApi : IBinaryApi
    {
        private readonly IBinaryMessageApi _binaryApi;

        private byte _requestResponseIdCounter=0;
        private Dictionary<byte,TaskCompletionSource<ArraySegment<byte>>> _responsesTcs = new();


        public BinaryApiOverBinaryMessageApi(IBinaryMessageApi binaryApi)
        {
            _binaryApi = binaryApi;
            
            _binaryApi.MainMessageHandler += OnMessage;
        }

        private async ValueTask OnMessage(ArraySegment<byte> arg)
        {
            switch (arg[arg.Count - 1])
            {
                case 0: // got message
                    {
                        if (MainMessageHandler == null)
                        {
                            throw new InvalidOperationException(
                                "MainMessageHandler is null, should not receive messages until handlers are set!");
                        }

                        await (MainMessageHandler?.Invoke(arg.Slice(0,arg.Count-1)) ?? new ValueTask());
                        break;
                    }
                case 1: // got message to respond to
                    {
                        if (MainMessageWithResponseHandler == null)
                        {
                            throw new InvalidOperationException(
                                "MainMessageWithResponseHandler is null, should not receive messages until handlers are set!");
                        }

                        var requestId = arg[arg.Count - 2];
                        var response = await MainMessageWithResponseHandler.Invoke(arg.Slice(0,arg.Count-2));
                        
                        response.Write(requestId);
                        response.Write(2);
                        
                        await _binaryApi.SendMessage(response);
                        break;
                    }
                case 2:// got response
                    {
                        var requestId = arg[arg.Count - 2];
                        _responsesTcs[requestId].SetResult(arg.Slice(0,arg.Count-2));
                        break;
                    }
            }
        }

        public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler { get; set; }
        
        public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            bytes.Write(0);
            await _binaryApi.SendMessage(bytes );
        }

        public Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler
        {
            get;
            set;
        }

        public async ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> bytes)
        {
            byte requestId;
            unchecked
            {
                requestId = _requestResponseIdCounter++;
            }
            
            var tcs= new TaskCompletionSource<ArraySegment<byte>>();
            _responsesTcs[requestId]=tcs;
            
            bytes.Write(requestId);
            bytes.Write(1);
            
            await _binaryApi.SendMessage(bytes);
            
            var response=await tcs.Task;
            _responsesTcs.Remove(requestId);
            return response;
        }
    }
}