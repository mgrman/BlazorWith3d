using System;

namespace BlazorWith3d.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class Blazor3DAppBindingAttribute : Attribute
    {
    }
    
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Blazor3DControllerAttribute : Attribute
    {
        public Blazor3DControllerAttribute(Type rendererType)
        {

        }
    }
    
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class Blazor3DRendererAttribute : Attribute
    {
        public Blazor3DRendererAttribute(Type controllerType)
        {

        }
    }
    
#if COMMON_DOTNET
#endif
#if COMMON_UNITY
#endif
}