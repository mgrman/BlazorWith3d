using System;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;
using MemoryPack;

namespace BlazorWith3d.ExampleApp.Client.Unity.Shared
{
#if COMMON_DOTNET
    [Blazor3DApp]
#endif
    public partial interface IBlocksOnGrid3DApp
    {
        public event Action<PerfCheckResponse> PerfCheckResponse;
        public event Action<AppInitialized> AppInitialized;
        public event Action<BlockPoseChangingMessage> BlockPoseChanging;
        public event Action<BlockPoseChangedMessage> BlockPoseChanged;

        Task PerfCheckRequest(PerfCheckRequest request);
        Task ControllerInitialized(ControllerInitializedRequest request);
        Task AddBlockTemplate(AddBlockTemplateMessage request);
        Task AddBlockInstance(AddBlockInstanceMessage request);
        Task RemoveBlock(RemoveBlockMessage request);
        Task RemoveBlockTemplate(RemoveBlockTemplateMessage request);
        Task StartDraggingBlock(StartDraggingBlockMessage request);
        Task BlockPoseChangingResponse(BlockPoseChangingResponse request);
    }


#if COMMON_UNITY
    [Unity3DApp(typeof(IBlocksOnGrid3DApp))]
    public partial interface IBlocksOnGridUnityApi
    {
    }
#endif
    [MemoryPackable]
    public partial class ControllerInitializedRequest 
    {
    }
    
    [MemoryPackable]
     public partial class PerfCheckRequest 
     {
         public float Aaa;
         public double Bbb;
         public decimal Ccc;
         public string? Ddd;
         public int Id;
     }

    [MemoryPackable]
     public partial class PerfCheckResponse 
     {
         public float Aaa;
         public double Bbb;
         public decimal Ccc;
         public string? Ddd;
         public int Id;
     }

    [MemoryPackable]
     public partial class AppInitialized 
     {
     }

    [MemoryPackable]
     public partial class AddBlockTemplateMessage 
     {
         public float SizeX;
         public float SizeY;
         public float SizeZ;
         public int TemplateId;
         public string? VisualsUri;
     }
     
    [MemoryPackable]
     public partial class AddBlockInstanceMessage 
     {
         public int BlockId;
         public float PositionX;
         public float PositionY;
         public float RotationZ;
         public int TemplateId;
     }

    [MemoryPackable]
     public partial class RemoveBlockMessage 
     {
         public int BlockId;
     }

    [MemoryPackable]
     public partial class RemoveBlockTemplateMessage 
     {
         public int TemplateId;
     }

    [MemoryPackable]
     public partial class StartDraggingBlockMessage 
     {
         public int BlockId;
         public int TemplateId;
     }

    [MemoryPackable]
     public partial class BlockPoseChangingResponse 
     {
         public int BlockId;
         public int ChangingRequestId;
         public bool IsValid;
         public float NewPositionX;
         public float NewPositionY;
         public float NewRotationZ;
     }

    [MemoryPackable]
     public partial class BlockPoseChangingMessage 
     {
         public int BlockId;
         public int ChangingRequestId;
         public float PositionX;
         public float PositionY;
         public float RotationZ;
     }

    [MemoryPackable]
     public partial class BlockPoseChangedMessage 
     {
         public int BlockId;
         public float PositionX;
         public float PositionY;
         public float RotationZ;
     }
}
