using System;
using System.Collections.Generic;
using System.Text;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class TypedMessageBlazorApi
    {
        private readonly IDictionary<Type, Func<int, string, Awaitable<string>>> _handlersWithResponse =
            new Dictionary<Type, Func<int, string, Awaitable<string>>>();

        private readonly IDictionary<Type, Action<string>> _handlers = new Dictionary<Type, Action<string>>();

        private readonly Dictionary<int, AwaitableCompletionSource<(string responseObjectJson, Type responseType)>>
            _responseTcs = new();


        public TypedMessageBlazorApi()
        {
            if (BlazorApi.OnHandleReceivedMessages != null)
            {
                throw new InvalidOperationException("There is already a handler for blazor messages!");
            }

            BlazorApi.OnHandleReceivedMessages = OnMessageReceived;
        }


        public void SendMessage<TMessage>(TMessage message) where TMessage : IMessageFromUnity<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();

            var messageString = JsonUtility.ToJson(message);
            var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

            try
            {
                SendMessageFromUnity(encodedMessage);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        public async Awaitable<TResponse> SendMessageWithResponse<TMessage, TResponse>(TMessage message)
            where TMessage : IMessageFromUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();

            var messageString = JsonUtility.ToJson(message);
            var (encodedMessage, msgId) = MessageTypeCache.EncodeMessageWithResponseJson<TMessage>(messageString);

            try
            {
                var tcs = new AwaitableCompletionSource<(string responseObjectJson, Type responseType)>();
                _responseTcs[msgId] = tcs;

                SendMessageFromUnity(encodedMessage);

                var (responseObjectJson, responseType) = await tcs.Awaitable;

                if (responseType != typeof(TResponse))
                {
                    throw new InvalidOperationException(
                        $"Unexpected response type! Expected {typeof(TResponse).Name} and received '{responseType}'!");
                }

                var response = JsonUtility.FromJson<TResponse>(responseObjectJson);
                if (response == null)
                {
                    throw new InvalidOperationException(
                        $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                throw;
            }
        }

        public void AddMessageWithResponseProcessCallback<TMessage, TResponse>(
            Func<TMessage, Awaitable<TResponse>> messageHandler) where TMessage : IMessageToUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();
            _handlersWithResponse[typeof(TMessage)] = async (responseId, objectJson) =>
            {
                var messageObject = JsonUtility.FromJson<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                var responseObject = await messageHandler(messageObject);

                var response = JsonUtility.ToJson(responseObject);
                return MessageTypeCache.EncodeResponseMessageJson<TResponse>(response, responseId);
            };
        }

        public void AddMessageProcessCallback<TMessage>(Action<TMessage> messageHandler)
            where TMessage : IMessageToUnity<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            _handlers[typeof(TMessage)] = objectJson =>
            {
                var messageObject = JsonUtility.FromJson<TMessage>(objectJson);
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
            BlazorApi.SendMessageFromUnity(Encoding.Unicode.GetBytes(msg));
        }

        protected async void OnMessageReceived(byte[] bytes)
        {
            var msg = Encoding.Unicode.GetString(bytes);
            
            var decoded = MessageTypeCache.DecodeMessageJson(msg);

            if (decoded == null)
            {
                if (TryHandleKnownMessages(msg))
                {
                    return;
                }

                throw new InvalidOperationException($"Non-encoded message received! '{msg}'");
            }

            if (decoded.Value.type == null)
            {
                Debug.LogWarning($"Unknown message type {decoded.Value.typeName}");
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
                    Debug.LogError($"Missing handler for message with response {decoded.Value.typeName}");
                    return;
                }

                var response = await handlerWithResponse(decoded.Value.respondWithId.Value, decoded.Value.objectJson);

                SendMessageFromUnity(response);
            }
            else
            {
                if (!_handlers.TryGetValue(decoded.Value.type, out var handler))
                {
                    Debug.LogError($"Missing handler for message {decoded.Value.typeName}");
                    return;
                }

                handler(decoded.Value.objectJson);
            }
        }

        private bool TryHandleKnownMessages(string msg)
        {
            switch (msg)
            {
                case "JS_INITIALIZED":
                    Debug.LogWarning($"JS_INITIALIZED received");
                    return true;
                default:
                    return false;
            }
        }
    }
}