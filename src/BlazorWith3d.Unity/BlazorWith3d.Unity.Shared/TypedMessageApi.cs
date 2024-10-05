// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
//
// namespace BlazorWith3d.Unity.Shared;
//
// public abstract class TypedMessageApi
// {
//     protected abstract string SerializeObject<T>(T obj);
//     protected abstract void SendMessage(string message);
//     
//     
//     private IDictionary<Type, Func<int, string, ValueTask<string>>> _handlersWithResponse =
//         new Dictionary<Type, Func<int, string, ValueTask<string>>>();
//
//     private IDictionary<Type, Action<string>> _handlers = new Dictionary<Type, Action<string>>();
//
//
//     private Dictionary<int, TaskCompletionSource<(string responseObjectJson, Type responseType)>> _responseTcs = new();
//
//
//
//     public async ValueTask SendMessage<TMessage>(TMessage message) where TMessage : IMessageToUnity<TMessage>
//     {
//         MessageTypeCache.AddTypeToCache<TMessage>();
//
//         var messageString = SerializeObject(message);
//         var encodedMessage = MessageTypeCache.EncodeMessageJson<TMessage>(messageString);
//
//         try
//         {
//             SendMessage(encodedMessage);
//         }
//         catch (Exception ex)
//         {
//             throw;
//         }
//     }
//
//     public async Task<TResponse> SendMessageWithResponse<TMessage, TResponse>(TMessage message)
//         where TMessage : IMessageToUnity<TMessage, TResponse>
//     {
//         MessageTypeCache.AddTypeToCache<TMessage>();
//         MessageTypeCache.AddTypeToCache<TResponse>();
//
//         var messageString = JsonConvert.SerializeObject(message);
//         var (encodedMessage, msgId) = MessageTypeCache.EncodeMessageWithResponseJson<TMessage>(messageString);
//
//         try
//         {
//             var tcs = new TaskCompletionSource<(string responseObjectJson, Type responseType)>();
//             _responseTcs[msgId] = tcs;
//             await base.SendMessageToUnityAsync(encodedMessage);
//
//             var (responseObjectJson, responseType) = await tcs.Task;
//
//             if (responseType != typeof(TResponse))
//             {
//                 throw new InvalidOperationException(
//                     $"Unexpected response type! Expected {typeof(TResponse).Name} and received '{responseType}'!");
//             }
//
//             var response = JsonConvert.DeserializeObject<TResponse>(responseObjectJson);
//             if (response == null)
//             {
//                 throw new InvalidOperationException(
//                     $"Response for {typeof(TMessage).Name} was not deserializable into {typeof(TResponse).Name}");
//             }
//
//             return response;
//         }
//         catch (Exception ex)
//         {
//             throw;
//         }
//     }
//
//     public void AddMessageWithResponseProcessCallback<TMessage, TResponse>(
//         Func<TMessage, ValueTask<TResponse>> messageHandler) where TMessage : IMessageFromUnity<TMessage, TResponse>
//     {
//         MessageTypeCache.AddTypeToCache<TMessage>();
//         MessageTypeCache.AddTypeToCache<TResponse>();
//         _handlersWithResponse[typeof(TMessage)] = async (responseId, objectJson) =>
//         {
//             var messageObject = JsonConvert.DeserializeObject<TMessage>(objectJson);
//             if (messageObject == null)
//             {
//                 throw new InvalidOperationException(
//                     $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
//             }
//
//             var responseObject = await messageHandler(messageObject);
//
//             var response = JsonConvert.SerializeObject(responseObject);
//             return MessageTypeCache.EncodeResponseMessageJson<TResponse>(response, responseId);
//         };
//     }
//
//     public void AddMessageProcessCallback<TMessage>(Func<TMessage, ValueTask> messageHandler)
//         where TMessage : IMessageFromUnity<TMessage>
//     {
//         MessageTypeCache.AddTypeToCache<TMessage>();
//         _handlers[typeof(TMessage)] = async objectJson =>
//         {
//             var messageObject = JsonConvert.DeserializeObject<TMessage>(objectJson);
//             if (messageObject == null)
//             {
//                 throw new InvalidOperationException(
//                     $"Message for {typeof(TMessage).Name} was not deserializable into {typeof(TMessage).Name}");
//             }
//
//             await messageHandler(messageObject);
//         };
//     }
//
//     protected override void OnMessageReceived(string msg)
//     {
//         var decoded = MessageTypeCache.DecodeMessageJson(msg);
//
//         if (decoded == null)
//         {
//             if (TryHandleKnownMessages(msg, out var knownMessageResponse))
//             {
//                 return;
//             }
//
//             throw new InvalidOperationException($"Non-encoded message received! {msg}");
//         }
//
//         if (decoded.Value.type == null)
//         {
//             Logger.LogWarning($"Unknown message type {decoded.Value.typeName}");
//             return;
//         }
//
//         if (decoded.Value.responseToMessageId != null)
//         {
//             _responseTcs[decoded.Value.responseToMessageId.Value]
//                 .SetResult((decoded.Value.objectJson, decoded.Value.type));
//         }
//         else if (decoded.Value.respondWithId != null)
//         {
//             if (!_handlersWithResponse.TryGetValue(decoded.Value.type, out var handlerWithResponse))
//             {
//                 Logger.LogError($"Missing handler for message with response {decoded.Value.typeName}");
//                 return;
//             }
//
//             Task.Run(async () =>
//             {
//                 var response = await handlerWithResponse(decoded.Value.respondWithId.Value, decoded.Value.objectJson);
//
//                 await base.SendMessageToUnityAsync(response);
//             });
//         }
//         else
//         {
//             if (!_handlers.TryGetValue(decoded.Value.type, out var handler))
//             {
//                 Logger.LogError($"Missing handler for message {decoded.Value.typeName}");
//                 return;
//             }
//
//             handler(decoded.Value.objectJson);
//         }
//     }
//
//     private bool TryHandleKnownMessages(string msg, out string response)
//     {
//         switch (msg)
//         {
//             case "UNITY_INITIALIZED":
//                 Logger.LogWarning($"UNITY_INITIALIZED received");
//                 response = "ACK";
//                 return true;
//             default:
//                 response = string.Empty;
//                 return false;
//         }
//     }
// }