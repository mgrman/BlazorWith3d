using System;
using System.Collections.Generic;
using System.Reflection;

namespace BlazorWith3d.Unity.Shared
{

    public static class MessageTypeCache
    {
        private static readonly Dictionary<string, Type> MessageTypeToType = new();
        private static readonly Dictionary<Type, string> TypeToMessageType = new();

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

            return $"{messageTypeName};{objectJson}";
        }

        public static (Type? type, string typeName, string objectJson)? DecodeMessageJson(string messageString)
        {
            var separatorIndex = messageString.IndexOf(';');
            if (separatorIndex == -1)
            {
                return null;
            }

            var typeName = messageString.Substring(0, separatorIndex);
            var objectJson = messageString.Substring(separatorIndex + 1);

            if (!MessageTypeToType.TryGetValue(typeName, out var type))
            {
                return (null, typeName, objectJson);
            }

            return (type, typeName, objectJson);
        }

    }
}