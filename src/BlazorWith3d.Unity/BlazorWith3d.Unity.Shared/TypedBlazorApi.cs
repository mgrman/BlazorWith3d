using System;
using System.Collections.Generic;
using System.Text;
using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity
{
    public abstract class TypedBlazorApi
    {
        private readonly IBlazorApi _blazorApi;

        private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

        public TypedBlazorApi(IBlazorApi blazorApi)
        {
            if (blazorApi.OnMessageFromBlazor != null)
                throw new InvalidOperationException("There is already a handler for blazor messages!");

            _blazorApi = blazorApi;

            blazorApi.OnMessageFromBlazor = OnMessageReceived;
        }

        protected abstract void LogError(Exception exception, string msg);
        protected abstract void LogError(string msg);
        protected abstract void LogWarning(string msg);
        protected abstract void Log(string msg);
        protected abstract byte[] SerializeObject<T>(T obj) where T : IMessageToBlazor;
        protected abstract object? DeserializeObject(byte[] json);

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
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");

                messageHandler(messageObject);
            };
        }

        protected void OnMessageReceived(byte[] messageBytes)
        {
            var decoded = DeserializeObject(messageBytes);
            if (decoded == null) throw new InvalidOperationException($"Non-encoded message received! {messageBytes}");

            if (!_handlers.TryGetValue(decoded.GetType(), out var handler))
            {
                LogError($"Missing handler for message {decoded.GetType()}");
                return;
            }

            handler(decoded);
        }
    }
}