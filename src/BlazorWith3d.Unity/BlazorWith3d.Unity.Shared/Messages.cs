using System;

namespace BlazorWith3d.Unity.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BlazorApiAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class UnityApiAttribute : Attribute
    {
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