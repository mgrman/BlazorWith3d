using System;

namespace BlazorWith3d.Shared
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Blazor3DAppAttribute : Attribute
    {
        public Blazor3DAppAttribute(Type serializerType)
        {
            
        }
    }
    
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Unity3DAppAttribute : Attribute
    {
        public Unity3DAppAttribute(Type serializerType)
        {
            
        }
    }

    public interface IMessageToBlazor
    {
    }

    public interface IMessageToUnity
    {
    }

    public interface IMessageToUnity<TResponse>
    {
    }
    
#if COMMON_DOTNET
#endif
#if COMMON_UNITY
#endif
}