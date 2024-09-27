# API

## Types.v1
- record PositionXY(float x, float y)
- record SizeXYZ(float x, float y, float z)
- record AngleZ(float value)
- record TemplateId(Guid id)
- record BlockId(Guid id)
- record BlockPose(PositionXY positionCenter, AngleZ rotation)

## Methods.v1
- void AddBlockTemplate(TemplateId id, SizeXYZ size, Uri visuals)
- void AddBlockInstance(BlockId id, BlockPose pose, TemplateId template)
- void RemoveBlock(BlockId id)
- void RemoveBlockTemplate(TemplateId id)
- void SubscribeToBlockPoseChanging(Func<BlockId, BlockPose, BlockPose?> onPoseValidation)
- void SubscribeToBlockPoseChanged(Action<BlockId, BlockPose> onPoseChanged)
- void StartDraggingBlock(BlockId id, TemplateId template)



## Methods.v2
- void AddBlockTemplate(int templateId, float sizeX, float sizeY,float sizeZ, string visualsUri)
- void AddBlockInstance(int blockId, float positionX, float positionY, float rotationZ, string template)
- void RemoveBlock(int blockId)
- void RemoveBlockTemplate(int templateId)
- void SubscribeToBlockPoseChanging(Func<int blockId, float positionX, float positionY, float rotationZ, (float positionX, float positionY, float rotationZ)> onPoseValidation)
- void SubscribeToBlockPoseChanged(Action<int blockId, float positionX, float positionY, float rotationZ> onPoseChanged)
- void StartDraggingBlock(int blockId, int templateId)