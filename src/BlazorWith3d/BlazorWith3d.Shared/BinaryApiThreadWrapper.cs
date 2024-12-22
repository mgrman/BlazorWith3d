using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{


    public class BinaryApiThreadWrapper : IBinaryApi
    {
        private readonly IBinaryApi _binaryApi;
        private readonly Func<Action, Task> _threadHandler;

        public BinaryApiThreadWrapper(IBinaryApi binaryApi, Func<Action, Task> threadHandler)
        {
            _binaryApi = binaryApi;
            _threadHandler = threadHandler;
        }

        public Action<byte[]>? MainMessageHandler
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
                        _threadHandler(() => value(msg));
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