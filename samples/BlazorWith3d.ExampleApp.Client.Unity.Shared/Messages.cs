using BlazorWith3d.Unity.Shared;
using MemoryPack;

namespace BlazorWith3d.ExampleApp.Client.Unity.Shared
{
    // ## Methods.v2
// - void AddBlockTemplate(int templateId, float sizeX, float sizeY,float sizeZ, string visualsUri)
// - void AddBlockInstance(int blockId, float positionX, float positionY, float rotationZ, int templateId)
// - void RemoveBlock(int blockId)
// - void RemoveBlockTemplate(int templateId)
// - void SubscribeToBlockPoseChanging(Func<int blockId, float positionX, float positionY, float rotationZ, (float positionX, float positionY, float rotationZ)> onPoseValidation)
// - void SubscribeToBlockPoseChanged(Action<int blockId, float positionX, float positionY, float rotationZ> onPoseChanged)
// - void StartDraggingBlock(int blockId, int templateId)

#if COMMON_DOTNET
[UnityApi]
public partial class MyUnityApi
{
}
#endif

#if COMMON_UNITY
    [BlazorApi]
    public partial class MyBlazorApi
    {
        
    }
#endif


    [MemoryPackable]
    public partial class PerfCheckRequest : IMessageToUnity
    {
        public float Aaa;
        public double Bbb;
        public decimal Ccc;
        public string? Ddd;
        public int Id;
    }

    [MemoryPackable]
    public partial class PerfCheckResponse : IMessageToBlazor
    {
        public float Aaa;
        public double Bbb;
        public decimal Ccc;
        public string? Ddd;
        public int Id;
    }

    [MemoryPackable]
    public partial class AppInitialized : IMessageToBlazor
    {
    }

    [MemoryPackable]
    public partial class AddBlockTemplateMessage : IMessageToUnity
    {
        public float SizeX;
        public float SizeY;
        public float SizeZ;
        public int TemplateId;
        public string? VisualsUri;
    }

    [MemoryPackable]
    public partial class AddBlockInstanceMessage : IMessageToUnity
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
        public int TemplateId;
    }

    [MemoryPackable]
    public partial class RemoveBlockMessage : IMessageToUnity
    {
        public int BlockId;
    }

    [MemoryPackable]
    public partial class RemoveBlockTemplateMessage : IMessageToUnity
    {
        public int TemplateId;
    }

    [MemoryPackable]
    public partial class StartDraggingBlockMessage : IMessageToUnity
    {
        public int BlockId;
        public int TemplateId;
    }

    [MemoryPackable]
    public partial class BlockPoseChangingResponse : IMessageToUnity
    {
        public int BlockId;
        public int ChangingRequestId;
        public bool IsValid;
        public float NewPositionX;
        public float NewPositionY;
        public float NewRotationZ;
    }

    [MemoryPackable]
    public partial class BlockPoseChangingMessage : IMessageToBlazor
    {
        public int BlockId;
        public int ChangingRequestId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }

    [MemoryPackable]
    public partial class BlockPoseChangedMessage : IMessageToBlazor
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }
#if COMMON_UNITY
#endif
}