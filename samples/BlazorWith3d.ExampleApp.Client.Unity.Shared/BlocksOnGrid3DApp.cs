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
    public partial class BlazorControllerInitialized : IMessageToUnity
    {
    }

    [MemoryPackable]
    public partial class PerfCheck : IMessageToUnity, IMessageToBlazor
    {
        public float Aaa;
        public double Bbb;
        public decimal Ccc;
        public string? Ddd;
        public int Id;
    }


    [MemoryPackable]
    public partial class UnityAppInitialized : IMessageToBlazor
    {
    }

    [MemoryPackable]
    public partial class AddBlockTemplate : IMessageToUnity
    {
        public float SizeX;
        public float SizeY;
        public float SizeZ;
        public int TemplateId;
        public string? VisualsUri;
    }

    [MemoryPackable]
    public partial class AddBlockInstance : IMessageToUnity
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
        public int TemplateId;
    }

    [MemoryPackable]
    public partial class RemoveBlock : IMessageToUnity
    {
        public int BlockId;
    }

    [MemoryPackable]
    public partial class RemoveBlockTemplate : IMessageToUnity
    {
        public int TemplateId;
    }

    [MemoryPackable]
    public partial class StartDraggingBlock : IMessageToUnity
    {
        public int BlockId;
        public int TemplateId;
    }

    [MemoryPackable]
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
    public partial class BlockPoseChanging : IMessageToBlazor
    {
        public int BlockId;
        public int ChangingRequestId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }

    [MemoryPackable]
    public partial class BlockPoseChanged : IMessageToBlazor
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }
}


