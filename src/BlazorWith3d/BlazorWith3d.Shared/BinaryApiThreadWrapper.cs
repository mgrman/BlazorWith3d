using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{


    public class BinaryApiThreadWrapper : IBinaryApi
    {
        private readonly IBinaryApi _binaryApi;
        private readonly Func<Func<ValueTask>, ValueTask> _threadHandler;

        public BinaryApiThreadWrapper(IBinaryApi binaryApi, Func<Func<ValueTask>, ValueTask> threadHandler)
        {
            _binaryApi = binaryApi;
            _threadHandler = threadHandler;
        }

        public Func<byte[], ValueTask>? MainMessageHandler
        {
            get => _binaryApi.MainMessageHandler;
            set
            {
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

        public ValueTask SendMessage(byte[] bytes)
        {
            return _binaryApi.SendMessage(bytes);
        }
    }
}