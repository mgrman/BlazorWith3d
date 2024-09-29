using System;

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

    // ReSharper disable UnusedTypeParameter
    public interface IMessageFromUnity<TMessageFromUnity, TResponseToUnity>
        // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToUnity<TMessageToUnity, TResponseFromUnity>
        // ReSharper restore UnusedTypeParameter
    {
    }
    
    public sealed class NoResponse
    {
        // TODO add explictly not waiting for response if Expected response is NoResponse

        public static readonly NoResponse Instance = new NoResponse();
    }
    
#if COMMON_DOTNET
#endif

    

#if COMMON_UNITY
#endif

}