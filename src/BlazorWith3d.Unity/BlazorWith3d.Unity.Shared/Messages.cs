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
    public interface IMessageFromUnity<TMessageFromUnity>
        // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToUnity<TMessageToUnity, TResponseFromUnity>
        // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToUnity<TMessageToUnity>
        // ReSharper restore UnusedTypeParameter
    {
    }
    
#if COMMON_DOTNET
#endif

    

#if COMMON_UNITY
#endif

}