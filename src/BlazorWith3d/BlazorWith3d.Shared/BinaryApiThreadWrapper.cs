using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{


    public class BinaryApiThreadWrapper : IBinaryMessageApi
    {
        private readonly IBinaryMessageApi _binaryApi;
        private Func<ArraySegment<byte>, ValueTask>? _originalMainMessageHandlerReference;
        private readonly Func<Func<ValueTask>, ValueTask> _threadHandler;

        public BinaryApiThreadWrapper(IBinaryMessageApi binaryApi, Func<Func<ValueTask>, ValueTask> threadHandler)
        {
            _binaryApi = binaryApi;
            _threadHandler = threadHandler;
        }

        public Func<ArraySegment<byte>, ValueTask>? MainMessageHandler
        {
            get => _originalMainMessageHandlerReference;
            set
            {
                _originalMainMessageHandlerReference = value;
                if (value == null)
                {
                    _binaryApi.MainMessageHandler = null;
                }
                else
                {
                    _binaryApi.MainMessageHandler = (msg) =>
                    {
                       return _threadHandler(() => value(msg));
                    };
                }
            }
        }

        public ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
            return _binaryApi.SendMessage(bytes);
        }
    }
}