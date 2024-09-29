using System;
using System.Collections.Generic;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public static class TypedMessageBlazorApi
    {
        private static readonly IDictionary<Type, Func<string, string>> Handlers =
            new Dictionary<Type, Func<string, string>>();

        public static async Awaitable<TResponse> SendMessage<TMessage, TResponse>(TMessage message)
            where TMessage : IMessageFromUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();

            var messageString = JsonUtility.ToJson(message);
            var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);

            var responseString = await BlazorApi.SendMessageFromUnity(encodedMessage);

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

        public static void AddMessageProcessCallback<TMessage, TResponse>(Func<TMessage, TResponse> messageHandler)
            where TMessage : IMessageToUnity<TMessage, TResponse>
        {
            MessageTypeCache.AddTypeToCache<TMessage>();
            MessageTypeCache.AddTypeToCache<TResponse>();
            Handlers[typeof(TMessage)] = objectJson =>
            {
                var messageObject = JsonUtility.FromJson<TMessage>(objectJson);
                if (messageObject == null)
                {
                    throw new InvalidOperationException(
                        $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
                }

                var responseObject = messageHandler(messageObject);

                var response = JsonUtility.ToJson(responseObject);
                return MessageTypeCache.EncodeMessageJson<TResponse>(response);
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

        internal static string HandleReceivedMessages(string msg)
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

            if (!Handlers.TryGetValue(decoded.Value.type, out var handler))
            {
                Debug.LogWarning($"Missing handler for message {decoded.Value.typeName}");
                return string.Empty;
            }

            var response = handler(decoded.Value.objectJson);
            return response;
        }
    }
}