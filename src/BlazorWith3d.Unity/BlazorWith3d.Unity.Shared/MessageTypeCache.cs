using System;
using System.Collections.Generic;
using System.Reflection;

namespace BlazorWith3d.Unity.Shared
{
    public static class MessageTypeCache
    {
        public const string ExceptionMessageType = "EXCEPTION";
        private static readonly Dictionary<string, Type> MessageTypeToType = new();
        private static readonly Dictionary<Type, string> TypeToMessageType = new();

        private static int _messageCounter = 0;

        private static class TypeCacheInitializer<T>
        {
            static TypeCacheInitializer()
            {
                var typeName = typeof(T).GetCustomAttribute<MessageTypeAttribute>()?.TypeName ?? typeof(T).Name;
                TypeToMessageType[typeof(T)] = typeName;
                MessageTypeToType[typeName] = typeof(T);
            }

            public static void Dummy()
            {
            }
        }

        public static void AddTypeToCache<T>()
        {
            TypeCacheInitializer<T>.Dummy();
        }

        public static string EncodeMessageJson<TMessage>(string objectJson)
        {
            if (!TypeToMessageType.TryGetValue(typeof(TMessage), out var messageTypeName))
            {
                throw new InvalidOperationException();
            }

            return $"V;{messageTypeName};{objectJson}";
        }

        public static string EncodeResponseMessageJson<TResponse>(string objectJson, int responseId)
        {
            return EncodeResponseMessageJson(typeof(TResponse), objectJson, responseId);
        }

        public static string EncodeResponseMessageJson(Type responseType,string objectJson, int responseId)
        {
            if (!TypeToMessageType.TryGetValue(responseType, out var messageTypeName))
            {
                throw new InvalidOperationException();
            }

            return $"R{responseId};{messageTypeName};{objectJson}";
        }

        public static (string msg, int msgId) EncodeMessageWithResponseJson<TMessage>(string objectJson)
        {
            if (!TypeToMessageType.TryGetValue(typeof(TMessage), out var messageTypeName))
            {
                throw new InvalidOperationException();
            }

            int msgId;
            unchecked
            {
                msgId = _messageCounter++;
            }

            return ($"M{msgId};{messageTypeName};{objectJson}",msgId);
        }
        
        public static string EncodeException(Exception ex)
        {
            return $"{ExceptionMessageType};{SerializeException(ex)}";
        }

        private static string SerializeException(Exception ex)
        {
            return
                $"{ex.GetType().FullName};{ex.Message}{(ex.InnerException != null ? ";" + SerializeException(ex.InnerException) : "")};{ex.StackTrace}";

        }

        public static (int? responseToMessageId,int? respondWithId,Type? type, string typeName, string objectJson)? DecodeMessageJson(string messageString)
        {
            var separatorIndex = messageString.IndexOf(';');
            if (separatorIndex == -1)
            {
                return null;
            }
            var secondSeparatorIndex = messageString.IndexOf(';', separatorIndex+1);
            if (secondSeparatorIndex == -1)
            {
                return null;
            }
            
            
            var responseString = messageString.Substring(0, separatorIndex);

            int? responseToMessageId;
            int? respondWithId;
            
            switch (responseString)
            {
                case "V":
                    responseToMessageId=null;
                    respondWithId=null;
                    break;
                case "M":
                    responseToMessageId=int.Parse(responseString.AsSpan(1));
                    respondWithId=null;
                    break;
                case "R":
                    responseToMessageId=null;
                    respondWithId=int.Parse(responseString.AsSpan(1));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var typeName = messageString.Substring(separatorIndex+1, secondSeparatorIndex-separatorIndex-1);
            var objectJson = messageString.Substring(secondSeparatorIndex + 1);

            if (typeName == ExceptionMessageType)
            {
                throw new BlazorApiException(objectJson);
            }

            if (!MessageTypeToType.TryGetValue(typeName, out var type))
            {
                return (responseToMessageId,respondWithId, null, typeName, objectJson);
            }

            return (responseToMessageId,respondWithId,  type, typeName, objectJson);
        }
    }

    public class BlazorApiException : Exception
    {
        public BlazorApiException(string message)
        :base(message)
        {
        }
    }
}