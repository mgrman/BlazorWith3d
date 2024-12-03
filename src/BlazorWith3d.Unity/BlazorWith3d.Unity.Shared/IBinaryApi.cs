using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity.Shared
{
    public interface IBinaryApi
    {
        // if null, messages are buffered
        Action<byte[]>? MainMessageHandler { get; set; }
        ValueTask SendMessage(byte[] bytes);
    }


    public class BackgroundMessageBuffer
    {
        protected readonly List<byte[]> _unhandledMessages= new();
        protected Action<byte[]>? _mainMessageHandler;

        public Action<byte[]>? MainMessageHandler
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
        
        protected  void InvokeMessages(Action<byte[]> handler)
        {
            var unhandledMessagesCopy = _unhandledMessages.ToList();
            _unhandledMessages.Clear();
            
            Task.Run(async () =>
            {
                foreach (var unhandledMessage in unhandledMessagesCopy)
                {
                    if (HandleThread == null)
                    {
                        handler?.Invoke(unhandledMessage);
                    }
                    else
                    {
                        await HandleThread(() => handler?.Invoke(unhandledMessage));
                    }
                }
            });
        }

        public async Task InvokeMessageAsync(byte[] message)
        {
            if (MainMessageHandler != null)
            {
                if (HandleThread == null)
                {
                    _mainMessageHandler?.Invoke(message);
                }
                else
                {
                    await HandleThread(() => _mainMessageHandler?.Invoke(message));
                }
            }
            else
            {
                _unhandledMessages.Add(message);
            }
        }

        public Func<Action,Task> HandleThread { get; set; }
    }

    public class ReceiveMessageBuffer
    {
        protected readonly List<byte[]> _unhandledMessages= new();
        protected Action<byte[]>? _mainMessageHandler;

        public Action<byte[]>? MainMessageHandler
        {
            get => _mainMessageHandler;
            set
            {
                _mainMessageHandler = value;

                if (value != null && _unhandledMessages.Any())
                {
                    foreach (var item in _unhandledMessages)
                    {
                        value.Invoke(item);
                    }
                    _unhandledMessages.Clear();
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

