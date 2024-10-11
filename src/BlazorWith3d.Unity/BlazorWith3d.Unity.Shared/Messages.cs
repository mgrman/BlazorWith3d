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
    public interface IMessageToBlazor<TResponseToUnity>
    // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToBlazor
    // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToUnity<TResponseFromUnity>
    // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    public interface IMessageToUnity
    // ReSharper restore UnusedTypeParameter
    {
    }

#if COMMON_DOTNET
#endif



#if COMMON_UNITY
#endif

}