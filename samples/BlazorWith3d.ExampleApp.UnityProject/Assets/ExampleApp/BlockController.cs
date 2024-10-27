using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExampleApp
{
    public class BlockController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,
        IInitializePotentialDragHandler
    {
        private static int _validationCounter;

        [SerializeField] private GameObject _backgroundPlane;

        [SerializeField] private GameObject _cubePlaceholderVisuals;

        [SerializeField] private int TemplateId;

        [SerializeField] private Vector3 Size;

        [SerializeField] private string? VisualsUri;
        private readonly AwaitableCompletionSource _dragMessageCounter = new();

        private IBlocksOnGridUnityApi _appApi;
        private Vector3 _dragOffset;

        private Plane _dragPlane;
        // {
        //     get
        //     {
        //         return transform.localRotation.eulerAngles.z;
        //     }
        //     set
        //     {
        //         var angles = transform.localRotation.eulerAngles;
        //         transform.localRotation = Quaternion.Euler(angles.x,angles.y,value);
        //     }
        // }

        private int _lastAppliedValidationResponse;
        private int _lastRequestedValidatioId;

        private int BlockId;

        private Vector2 Position;
        // {
        //     get
        //     {
        //         return new Vector2(transform.localPosition.x, transform.localPosition.y);
        //     }
        //     set
        //     {
        //         transform.localPosition = new Vector3(value.x,value.y,transform.localPosition.z);
        //     }
        // }

        private float RotationZ;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"OnBeginDrag  {BlockId}");
        }


        public async void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"OnDrag  {BlockId}");

            var ray = Camera.main.ScreenPointToRay(eventData.position);

            if (!_dragPlane.Raycast(ray, out var distance)) return;
            Debug.Log($"OnDrag  {BlockId} {distance}");

            var dragPlaneHit = _backgroundPlane.transform.InverseTransformPoint(ray.GetPoint(distance));

            var newPosition = (dragPlaneHit - _dragOffset).xy();
            Debug.Log($"OnDrag  {BlockId} {newPosition}");


            var newValidationId = _validationCounter++;
            _lastRequestedValidatioId = newValidationId;
            _dragMessageCounter.Reset();

            await _appApi.InvokeBlockPoseChanging(
                new BlockPoseChanging
                {
                    BlockId = BlockId,
                    PositionX = newPosition.x,
                    PositionY = newPosition.y,
                    RotationZ = RotationZ,
                    ChangingRequestId = newValidationId
                });
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag  {BlockId}");

            await _dragMessageCounter.Awaitable;
            await _appApi.InvokeBlockPoseChanged(
                new BlockPoseChanged
                {
                    BlockId = BlockId,
                    PositionX = Position.x,
                    PositionY = Position.y,
                    RotationZ = RotationZ
                });
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Debug.Log($"OnInitializePotentialDrag  {BlockId}");


            _dragPlane = new Plane(_backgroundPlane.transform.forward, eventData.pointerCurrentRaycast.worldPosition);

            var localHit =
                _backgroundPlane.transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);

            _dragOffset = localHit - transform.localPosition;

            _validationCounter = 0;
            _lastAppliedValidationResponse = -1;
            _lastRequestedValidatioId = -1;
        }

        public void Initialize(AddBlockTemplate msg, GameObject backgroundPlane, IBlocksOnGridUnityApi appApi)
        {
            TemplateId = msg.TemplateId;
            Size = new Vector3(msg.SizeX, msg.SizeY, msg.SizeZ);
            VisualsUri = msg.VisualsUri;
            _backgroundPlane = backgroundPlane;

            _appApi = appApi;

            _cubePlaceholderVisuals.transform.localScale = Size;
            _cubePlaceholderVisuals.transform.localPosition = new Vector3(0, 0, Size.z / 2);
        }

        public BlockController CreateInstance(AddBlockInstance msg)
        {
            var blockGo = Instantiate(this, _backgroundPlane.transform);
            blockGo._appApi = _appApi;

            blockGo.InitializeInstance(msg);

            return blockGo;
        }

        private void InitializeInstance(AddBlockInstance msg)
        {
            BlockId = msg.BlockId;
            Position = new Vector3(msg.PositionX, msg.PositionY, 0);
            RotationZ = msg.RotationZ;

            UpdatePose();
        }

        private void UpdatePose()
        {
            transform.localPosition = new Vector3(Position.x, Position.y, 0);
            transform.localRotation = Quaternion.Euler(0, 0, RotationZ);
        }

        public void OnBlockPoseChangingResponse(BlockPoseChangeValidated newPose)
        {
            if (newPose.ChangingRequestId < _lastAppliedValidationResponse) return;


            _lastAppliedValidationResponse = newPose.ChangingRequestId;

            Position = new Vector2(newPose.NewPositionX, newPose.NewPositionY);
            RotationZ = newPose.NewRotationZ;

            UpdatePose();

            if (newPose.ChangingRequestId == _lastRequestedValidatioId) _dragMessageCounter.SetResult();
        }
    }
}