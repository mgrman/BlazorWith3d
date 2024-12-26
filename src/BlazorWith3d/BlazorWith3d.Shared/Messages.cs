using System;

namespace BlazorWith3d.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Blazor3DAppAttribute : Attribute
    {
        public Blazor3DAppAttribute(bool generateObjectApi = false)
        {
            
        }
    }
    
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Unity3DAppAttribute : Attribute
    {
        public Unity3DAppAttribute(bool generateObjectApi = false)
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