using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlazorWith3d.Unity.Shared
{
    public class MessageTypeAttribute : Attribute
    {
        public string? TypeName { get; }

        public MessageTypeAttribute()
        {
        }
        
        public MessageTypeAttribute(string? typeName)
        {
            TypeName = typeName;
        }
    }

    public static class SharedMessageTypesMap
    {
        public static IReadOnlyDictionary<string, Type> MessageTypeToType { get; } = typeof(MessageTypeAttribute)
            .Assembly
            .GetTypes()
            .Select(o => (attribute: o.GetCustomAttribute<MessageTypeAttribute>(), type: o))
            .Where(o => o.attribute.TypeName != null)
            .ToDictionary(o => o.attribute.TypeName ?? o.type.Name, o => o.type);

        public static IReadOnlyDictionary<Type, string> TypeToMessageType { get; } = typeof(MessageTypeAttribute)
            .Assembly
            .GetTypes()
            .Select(o => (attribute: o.GetCustomAttribute<MessageTypeAttribute>(), type: o))
            .Where(o => o.attribute.TypeName != null)
            .ToDictionary(o => o.type, o => o.attribute.TypeName ?? o.type.Name);
    }

    public interface IMessage<T, T1>
    {
    }
    
    [MessageType]
    public sealed class Message1:IMessage<Message1,MessageResponse1>
    {
    }

    public sealed class MessageResponse1
    {
    }

    
#if COMMON_DOTNET
#endif

#if COMMON_UNITY
#endif

}