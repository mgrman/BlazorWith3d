using System;
using System.Collections.Generic;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public static class TypedMessageBlazorApi
    {
        private static readonly IDictionary<Type, Func<string, string>> HandlersWithResponse =
            new Dictionary<Type, Func<string, string>>();
        private static readonly IDictionary<Type, Action<string>> Handlers =
            new Dictionary<Type, Action<string>>();

        public static void SendMessage<TMessage>(TMessage message)
            where TMessage : IMessageFromUnity<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();

            var messageString = JsonUtility.ToJson(message);
            var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

            BlazorApi.SendMessageFromUnity(encodedMessage);
        }

        public static async Awaitable<TResponse> SendMessage<TMessage, TResponse>(TMessage message)
            where TMessage : IMessageFromUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();

            var messageString = JsonUtility.ToJson(message);
            var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);
            
            var responseString = await BlazorApi.SendMessageWithResponseFromUnityAsync(encodedMessage);

            var decoded = MessageTypeCache.DecodeMessageJson(responseString);

            if (decoded == null)
            {
                throw new InvalidOperationException(
                    $"Response for {typeof(TMessage).Name} was not encoded correctly! {responseString}");
            }

            if (decoded.Value.type != typeof(TResponse))
            {
                throw new InvalidOperationException(
                    $"Unexpected response type! Expected {typeof(TResponse).Name} and received '{decoded.Value.typeName}'!");
            }

            var response = JsonUtility.FromJson<TResponse>(decoded.Value.objectJson);
            if (response == null)
            {
                throw new InvalidOperationException(
                    $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
            }

            return response;
        }

        public static void SimulateMessage(string message)
        {
            HandleReceivedMessages(message);
        }

        public static string SimulateMessageWithResponse(string message)
        {
            return HandleReceivedMessagesWithResponse(message);
        }

        public static void AddMessageWithResponseProcessCallback<TMessage, TResponse>(Func<TMessage, TResponse> messageHandler)
            where TMessage : IMessageToUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();
            HandlersWithResponse[typeof(TMessage)] = objectJson =>
            {
                var messageObject = JsonUtility.FromJson<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                try
                {
                    var responseObject = messageHandler(messageObject);

                    var response = JsonUtility.ToJson(responseObject);
                    return MessageTypeCache.EncodeMessageJson<TResponse>(response);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    throw;
                }
            };
        }

        public static void AddMessageProcessCallback<TMessage>(Action<TMessage> messageHandler)
            where TMessage : IMessageToUnity<TMessage>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            Handlers[typeof(TMessage)] = objectJson =>
            {
                var messageObject = JsonUtility.FromJson<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
                }

                try
                {
                    messageHandler(messageObject);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    throw;
                }
            };
        }

        private static bool TryHandleKnownMessages(string msg, out string response)
        {
            switch (msg)
            {
                case "JS_INITIALIZED":
                    Debug.LogWarning($"JS_INITIALIZED received");
                    response = "ACK";
                    return true;
                default:
                    response = string.Empty;
                    return false;
            }
        }

        internal static void HandleReceivedMessages(string msg)
        {
            var decoded = MessageTypeCache.DecodeMessageJson(msg);

            if (decoded == null)
            {
                if (TryHandleKnownMessages(msg, out var knownMessageResponse))
                {
                    return;
                }

                throw new InvalidOperationException($"Non-encoded message received! {msg}");
            }

            if (decoded.Value.type == null)
            {
                Debug.LogWarning($"Unknown message type {decoded.Value.typeName}");
                return;
            }

            if (!Handlers.TryGetValue(decoded.Value.type, out var handler))
            {
                Debug.LogWarning($"Missing handler for message {decoded.Value.typeName}");
                return;
            }

            handler(decoded.Value.objectJson);
        }

        internal static string HandleReceivedMessagesWithResponse(string msg)
        {
            var decoded = MessageTypeCache.DecodeMessageJson(msg);

            if (decoded == null)
            {
                if (TryHandleKnownMessages(msg, out var knownMessageResponse))
                {
                    return knownMessageResponse;
                }

                throw new InvalidOperationException($"Non-encoded message received! {msg}");
            }

            if (decoded.Value.type == null)
            {
                Debug.LogWarning($"Unknown message type {decoded.Value.typeName}");
                return string.Empty;
            }

            if (!HandlersWithResponse.TryGetValue(decoded.Value.type, out var handler))
            {
                Debug.LogWarning($"Missing handler for message {decoded.Value.typeName}");
                return string.Empty;
            }

            var response = handler(decoded.Value.objectJson);
            return response;
        }
    }
}