using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity
{

    public abstract class TypedUnityApi
    {
        private readonly IUnityApi _unityApi;

        private IDictionary<Type, Func<int, string, Task<string>>> _handlersWithResponse =
            new Dictionary<Type, Func<int, string, Task<string>>>();

        private IDictionary<Type, Action<string>> _handlers = new Dictionary<Type, Action<string>>();

        private Dictionary<int, TaskCompletionSource<(string responseObjectJson, Type responseType)>> _responseTcs =
            new();

        public TypedUnityApi(IUnityApi unityApi)
        {
            _unityApi = unityApi;
            _unityApi.OnMessageFromUnity = OnMessageBytesReceived;
        }


        protected abstract void LogError(Exception exception, string msg);
        protected abstract void LogError(string msg);
        protected abstract void LogWarning(string msg);
        protected abstract void Log(string msg);
        protected abstract string SerializeObject<T>(T obj);
        protected abstract T DeserializeObject<T>(string json);

        public async Task SendMessage<TMessage>(TMessage message) where TMessage : IMessageToUnity<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();

            var messageString = SerializeObject(message);
            var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

            try
            {
                SendMessageToUnity(encodedMessage);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to send message to Unity");
                throw;
            }
        }

        public async Task<TResponse> SendMessageWithResponse<TMessage, TResponse>(TMessage message)
            where TMessage : IMessageToUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();

            var messageString = SerializeObject(message);
            var (encodedMessage, msgId) = MessageTypeCache.EncodeMessageWithResponseJson<TMessage>(messageString);

            try
            {
                var tcs = new TaskCompletionSource<(string responseObjectJson, Type responseType)>();
                _responseTcs[msgId] = tcs;
                SendMessageToUnity(encodedMessage);

                var (responseObjectJson, responseType) = await tcs.Task;

                if (responseType != typeof(TResponse))
                {
                    throw new InvalidOperationException(
                        $"Unexpected response type! Expected {typeof(TResponse).Name} and received '{responseType}'!");
                }

                var response = DeserializeObject<TResponse>(responseObjectJson);
                if (response == null)
                {
                    throw new InvalidOperationException(
                        $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
                }

                return response;
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to send message to Unity");
                throw;
            }
        }

        public void AddMessageWithResponseProcessCallback<TMessage, TResponse>(
            Func<TMessage, Task<TResponse>> messageHandler) where TMessage : IMessageToBlazor<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();
            _handlersWithResponse[typeof(TMessage)] = async (responseId, objectJson) =>
            {
                var messageObject = DeserializeObject<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                var responseObject = await messageHandler(messageObject);

                var response = SerializeObject(responseObject);
                return MessageTypeCache.EncodeResponseMessageJson<TResponse>(response, responseId);
            };
        }

        public void AddMessageProcessCallback<TMessage>(Action<TMessage> messageHandler)
            where TMessage : IMessageToBlazor<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            _handlers[typeof(TMessage)] = objectJson =>
            {
                var messageObject = DeserializeObject<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                messageHandler(messageObject);
            };
        }

        protected void OnMessageBytesReceived(byte[] messageBytes)
        {
            OnMessageReceived(Encoding.Unicode.GetString(messageBytes));
        }


        protected void SendMessageToUnity(string message)
        {
            _unityApi.SendMessageToUnity(Encoding.Unicode.GetBytes(message));
        }

        protected virtual async void OnMessageReceived(string msg)
        {
            var decoded = MessageTypeCache.DecodeMessageJson(msg);

            if (decoded == null)
            {
                if (TryHandleKnownMessages(msg))
                {
                    return;
                }

                throw new InvalidOperationException($"Non-encoded message received! {msg}");
            }

            if (decoded.Value.type == null)
            {
                LogWarning($"Unknown message type {decoded.Value.typeName}");
                return;
            }

            if (decoded.Value.responseToMessageId != null)
            {
                _responseTcs[decoded.Value.responseToMessageId.Value]
                    .SetResult((decoded.Value.objectJson, decoded.Value.type));
            }
            else if (decoded.Value.respondWithId != null)
            {
                if (!_handlersWithResponse.TryGetValue(decoded.Value.type, out var handlerWithResponse))
                {
                    LogError($"Missing handler for message with response {decoded.Value.typeName}");
                    return;
                }


                var response = await handlerWithResponse(decoded.Value.respondWithId.Value, decoded.Value.objectJson);

                SendMessageToUnity(response);
            }
            else
            {
                if (!_handlers.TryGetValue(decoded.Value.type, out var handler))
                {
                    LogError($"Missing handler for message {decoded.Value.typeName}");
                    return;
                }

                handler(decoded.Value.objectJson);
            }
        }

        private bool TryHandleKnownMessages(string msg)
        {
            switch (msg)
            {
                case "UNITY_INITIALIZED":
                    Log($"UNITY_INITIALIZED received");
                    return true;
                default:
                    return false;
            }
        }
    }
}