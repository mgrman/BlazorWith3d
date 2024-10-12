using System;
using MemoryPack;

namespace BlazorWith3d.Unity.Shared
{
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