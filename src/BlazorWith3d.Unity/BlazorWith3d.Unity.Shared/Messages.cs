using System;

namespace BlazorWith3d.Unity.Shared
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Blazor3DAppAttribute : Attribute
    {
    }
    
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Unity3DAppAttribute : Attribute
    {
        public Unity3DAppAttribute(Type blazor3DApType)
        {
            
        }
    }

    public interface IMessageToBlazor
    {
    }

    public interface IMessageToUnity
    {
    }
    
#if COMMON_DOTNET
#endif
#if COMMON_UNITY
#endif
}