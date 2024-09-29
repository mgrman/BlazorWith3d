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
        public int TemplateId 
#if COMMON_UNITY
;
#elif COMMON_DOTNET
{get;set;}
#endif
        public float SizeX
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float SizeY
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float SizeZ
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public string? VisualsUri
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class AddBlockInstanceMessage : IMessageToUnity<AddBlockInstanceMessage, NoResponse>
    {
        public int BlockId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionX
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionY
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float RotationZ
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public int TemplateId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class RemoveBlockMessage : IMessageToUnity<RemoveBlockMessage, NoResponse>
    {
        public int BlockId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class RemoveBlockTemplateMessage : IMessageToUnity<RemoveBlockTemplateMessage, NoResponse>
    {
        public int TemplateId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class StartDraggingBlockMessage : IMessageToUnity<StartDraggingBlockMessage, NoResponse>
    {
        public int TemplateId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public int BlockId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class PoseChangeResponse
    {
        public bool IsValid
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float? NewPositionX
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float? NewPositionY
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float? NewRotationZ
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class BlockPoseChangingMessage : IMessageFromUnity<BlockPoseChangingMessage, PoseChangeResponse>
    {
        public int BlockId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionX
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionY
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float RotationZ
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }

    
    public sealed class BlockPoseChangedMessage : IMessageFromUnity<BlockPoseChangedMessage, NoResponse>
    {
        public int BlockId
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionX
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float PositionY
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
        public float RotationZ
#if COMMON_UNITY
;
#elif COMMON_DOTNET
        {get;set;}
#endif
    }


#if COMMON_UNITY
#endif

#if COMMON_DOTNET
#endif
}