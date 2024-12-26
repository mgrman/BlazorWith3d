using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApi
    {
        // if null, messages are buffered
        Func<byte[],ValueTask>? MainMessageHandler { get; set; }
        ValueTask SendMessage(byte[] bytes);
    }

    public class BackgroundMessageBuffer
    {
        protected readonly List<byte[]> _unhandledMessages= new();
        protected Func<byte[],ValueTask>? _mainMessageHandler;

        public Func<byte[],ValueTask>? MainMessageHandler
        {
            get => _mainMessageHandler;
            set
            {
                _mainMessageHandler = value;

                if (value != null && _unhandledMessages.Any())
                {
                    InvokeMessages(value);
                }
            }
        }
        
        protected  void InvokeMessages(Func<byte[],ValueTask> handler)
        {
            var unhandledMessagesCopy = _unhandledMessages.ToList();
            _unhandledMessages.Clear();
            
            Task.Run(async () =>
            {
                foreach (var unhandledMessage in unhandledMessagesCopy)
                {
                    if (HandleThread == null)
                    {
                       await handler.Invoke(unhandledMessage);
                    }
                    else
                    {
                        await HandleThread(async () => await handler.Invoke(unhandledMessage));
                    }
                }
            });
        }

        public Func<Func<ValueTask>,Task> HandleThread { get; set; }
    }

    public class ReceiveMessageBuffer
    {
        protected readonly List<byte[]> _unhandledMessages= new();
        protected Func<byte[], ValueTask>? _mainMessageHandler;

        public Func<byte[], ValueTask>? MainMessageHandler
        {
            get => _mainMessageHandler;
            set
            {
                _mainMessageHandler = value;

                if (value != null && _unhandledMessages.Any())
                {
                    Task.Run(async () =>
                    {
                        foreach (var item in _unhandledMessages)
                        {
                            await value.Invoke(item);
                        }
                        _unhandledMessages.Clear();
                    });
                }
            }
        }

        public void InvokeMessage(byte[] message)
        {
            if (_mainMessageHandler != null)
            {
                _mainMessageHandler.Invoke(message);
            }
            else
            {
                _unhandledMessages.Add(message);
            }
        }
    }
}

