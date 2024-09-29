using System;
using BlazorWith3d.Unity.Shared;

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

    public sealed class AddBlockTemplateMessage : IMessageToUnity<AddBlockTemplateMessage, NoResponse>
    {
        public int TemplateId;
        public float SizeX;
        public float SizeY;
        public float SizeZ;
        public string? VisualsUri;
    }

    
    public sealed class AddBlockInstanceMessage : IMessageToUnity<AddBlockInstanceMessage, NoResponse>
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
        public int TemplateId;
    }

    
    public sealed class RemoveBlockMessage : IMessageToUnity<RemoveBlockMessage, NoResponse>
    {
        public int BlockId;
    }

    
    public sealed class RemoveBlockTemplateMessage : IMessageToUnity<RemoveBlockTemplateMessage, NoResponse>
    {
        public int TemplateId;
    }

    
    public sealed class StartDraggingBlockMessage : IMessageToUnity<StartDraggingBlockMessage, NoResponse>
    {
        public int TemplateId;
        public int BlockId;
    }

    
    public sealed class PoseChangeResponse
    {
        public bool IsValid;
        public float? NewPositionX;
        public float? NewPositionY;
        public float? NewRotationZ;
    }

    
    public sealed class BlockPoseChangingMessage : IMessageFromUnity<BlockPoseChangingMessage, PoseChangeResponse>
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }

    
    public sealed class BlockPoseChangedMessage : IMessageFromUnity<BlockPoseChangedMessage, NoResponse>
    {
        public int BlockId;
        public float PositionX;
        public float PositionY;
        public float RotationZ;
    }


#if COMMON_UNITY
#endif

#if COMMON_DOTNET
#endif
}