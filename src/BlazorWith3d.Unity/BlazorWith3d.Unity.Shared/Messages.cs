using System;
using MemoryPack;

namespace BlazorWith3d.Unity.Shared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class BlazorApiAttribute : Attribute
    {
        public BlazorApiAttribute()
        {
        }
    }
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UnityApiAttribute : Attribute
    {
        public UnityApiAttribute()
        {
        }
    }
    
    // ReSharper disable UnusedTypeParameter
    [MemoryPackable(GenerateType.NoGenerate)]
    public partial interface IMessageToBlazor
    // ReSharper restore UnusedTypeParameter
    {
    }

    // ReSharper disable UnusedTypeParameter
    [MemoryPackable(GenerateType.NoGenerate)]
    public partial  interface IMessageToUnity
    // ReSharper restore UnusedTypeParameter
    {
    }

#if COMMON_DOTNET
#endif



#if COMMON_UNITY
#endif

}