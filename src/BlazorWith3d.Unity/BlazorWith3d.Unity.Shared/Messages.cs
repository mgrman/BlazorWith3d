using System;
using MemoryPack;

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

    [MemoryPackable(GenerateType.NoGenerate)]
    public partial interface IMessageToBlazor
    {
    }

    [MemoryPackable(GenerateType.NoGenerate)]
    public partial interface IMessageToUnity
    {
    }
#if COMMON_DOTNET
#endif
#if COMMON_UNITY
#endif
}