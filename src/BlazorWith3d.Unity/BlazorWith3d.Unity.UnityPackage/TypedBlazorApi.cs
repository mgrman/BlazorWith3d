using System;
using System.Collections.Generic;
using System.Text;
using BlazorWith3d.Unity.Shared;
using MemoryPack;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    // not in Shared package, as it uses too many Unity specific APIs, mainly Awaitables
    public class TypedBlazorApi
    {
        private readonly IBlazorApi _blazorApi;

        private IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

        public TypedBlazorApi(IBlazorApi blazorApi)
        {
            if (blazorApi.OnMessageFromBlazor != null)
            {
                throw new InvalidOperationException("There is already a handler for blazor messages!");
            }

            _blazorApi = blazorApi;

            blazorApi.OnMessageFromBlazor = OnMessageReceived;
        }

        protected byte[] SerializeObject<T>(T obj) where T : IMessageToBlazor
        {
            return MemoryPackSerializer.Serialize<IMessageToBlazor>(obj);
        }

        protected object DeserializeObject(byte[] obj)
        {
             return MemoryPackSerializer.Deserialize<IMessageToUnity>(obj);
        }
        
        protected void LogError(Exception exception, string msg)
        {
            Debug.LogException(exception);
            Debug.LogError(msg);
        }

        protected void LogError(string msg)
        {
            Debug.LogError( msg);
        }

        protected void LogWarning(string msg)
        {
            Debug.LogWarning( msg);
        }

        protected void Log(string msg)
        {
            Debug.Log( msg);
        }

        public void SendMessage<TMessage>(TMessage message) where TMessage : IMessageToBlazor
        {
            var encodedMessage = SerializeObject(message);

            try
            {
                _blazorApi.SendMessageToBlazor(encodedMessage);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to send message to Unity");
                throw;
            }
        }

        public void AddMessageProcessCallback<TMessage>(Action<TMessage> messageHandler)
            where TMessage : IMessageToUnity
        {
            _handlers[typeof(TMessage)] = objectJson =>
            {
                var messageObject = (TMessage)objectJson;
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                messageHandler(messageObject);
            };
        }

        protected void SendMessageFromUnity(string msg)
        {
            _blazorApi.SendMessageToBlazor(Encoding.Unicode.GetBytes(msg));
        }

        protected async void OnMessageReceived(byte[] messageBytes)
        {
            var decoded = DeserializeObject(messageBytes);
            if (decoded == null)
            {
                throw new InvalidOperationException($"Non-encoded message received! {messageBytes}");
            }

            if (!_handlers.TryGetValue(decoded.GetType(), out var handler))
            {
                LogError($"Missing handler for message {decoded.GetType()}");
                return;
            }

            handler(decoded);
        }
    }
}