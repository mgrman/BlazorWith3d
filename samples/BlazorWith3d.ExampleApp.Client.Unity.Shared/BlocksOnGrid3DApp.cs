using System;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;
using MemoryPack;

namespace BlazorWith3d.ExampleApp.Client.Unity.Shared
{
#if COMMON_DOTNET
    [Blazor3DApp]
#endif
    public partial class BlocksOnGrid3DApp
    {
    }


#if COMMON_UNITY
    [Unity3DApp]
    public partial class BlocksOnGridUnityApi
    {
    }
#endif

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlazorControllerInitialized : IMessageToUnity
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class PerfCheck : IMessageToUnity, IMessageToBlazor
    {
        public float Aaa;
        public double Bbb;
        public long Ccc;
        public string? Ddd;
        public int Id;
    }


    [MemoryPackable]
    [GenerateTypeScript]
    public partial class UnityAppInitialized : IMessageToBlazor
    {
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockTemplate : IMessageToUnity
    {
        public float SizeX;
        public float SizeY;
        public float SizeZ;
        public int TemplateId;
        public string? VisualsUri;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class AddBlockInstance : IMessageToUnity
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
        public int TemplateId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlock : IMessageToUnity
    {
        public int BlockId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class RemoveBlockTemplate : IMessageToUnity
    {
        public int TemplateId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class StartDraggingBlock : IMessageToUnity
    {
        public int BlockId;
        public int TemplateId;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlockPoseChangeValidated : IMessageToUnity
    {
        public int BlockId;
        public int ChangingRequestId;
        public bool IsValid;
        public float NewPositionX;
        public float NewPositionY;
        public float NewRotationZ;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlockPoseChanging : IMessageToBlazor
    {
        public int BlockId;
        public int ChangingRequestId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }

    [MemoryPackable]
    [GenerateTypeScript]
    public partial class BlockPoseChanged : IMessageToBlazor
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }
}


