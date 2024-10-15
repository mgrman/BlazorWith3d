using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity.Shared
{
    public abstract class TypedUnityApi
    {
        private readonly IUnityApi _unityApi;

        private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

        public TypedUnityApi(IUnityApi unityApi)
        {
            _unityApi = unityApi;
            _unityApi.OnMessageFromUnity = OnMessageReceived;
        }

        protected abstract void LogError(Exception exception, string msg);
        protected abstract void LogError(string msg);
        protected abstract void LogWarning(string msg);
        protected abstract void Log(string msg);
        protected abstract byte[] SerializeObject<T>(T obj) where T : IMessageToUnity;
        protected abstract object? DeserializeObject(byte[] json);

        public async Task SendMessage<TMessage>(TMessage message) where TMessage : IMessageToUnity
        {
            var encodedMessage = SerializeObject(message);

            try
            {
                await _unityApi.SendMessageToUnity(encodedMessage);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to send message to Unity");
                throw;
            }
        }

        public void AddMessageProcessCallback<TMessage>(Action<TMessage> messageHandler)
            where TMessage : IMessageToBlazor
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