using System;
using System.Threading;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExampleApp
{
    public class BlockController:MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        [SerializeField]
        private GameObject _backgroundPlane;
        
        [SerializeField]
        private GameObject _cubePlaceholderVisuals;

        [SerializeField]
        private int TemplateId;
        
        [SerializeField]
        private Vector3 Size;
        
        [SerializeField]
        private string? VisualsUri;

        private TypedMessageBlazorApi _blazorApi;

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
        
        private AwaitableCompletionSource _dragMessageCounter = new AwaitableCompletionSource();

        private CancellationTokenSource _lastDragCts;
        
        private Plane _dragPlane;
        private Vector3 _dragOffset; 

        public void Initialize(AddBlockTemplateMessage msg, GameObject backgroundPlane, TypedMessageBlazorApi blazorApi )
        {
            TemplateId = msg.TemplateId;
            Size = new(msg.SizeX, msg.SizeY, msg.SizeZ);
            VisualsUri = msg.VisualsUri;
            _backgroundPlane = backgroundPlane;

            _blazorApi = blazorApi;

            _cubePlaceholderVisuals.transform.localScale = Size;
            _cubePlaceholderVisuals.transform.localPosition = new Vector3(0, Size.y / 2, 0);
        }

        public BlockController CreateInstance(AddBlockInstanceMessage msg)
        {
            var blockGo=GameObject.Instantiate(this, _backgroundPlane.transform  );
            blockGo._blazorApi = _blazorApi;
            
            blockGo.InitializeInstance(msg);

            return blockGo;
        }

        private void InitializeInstance(AddBlockInstanceMessage msg)
        {
            BlockId = msg.BlockId;
            Position=new Vector3(msg.PositionX, msg.PositionY,0);
            RotationZ = msg.RotationZ;

            UpdatePose();
        }

        private void UpdatePose()
        {
            transform.localPosition= new Vector3(Position.x, Position.y,0);
            transform.localRotation= Quaternion.Euler(0, 0, RotationZ); 
        }


        public async void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"OnDrag  {BlockId}");

            var ray = Camera.main.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition);

            if (!_dragPlane.Raycast(ray, out var distance))
            {
                return;
            }

            var dragPlaneHit = _backgroundPlane.transform.InverseTransformPoint(ray.GetPoint(distance));

            var newPosition = (dragPlaneHit - _dragOffset).xy();
            
            _lastDragCts?.Cancel();
            var cts=new CancellationTokenSource();
            _lastDragCts = cts;
            
            _dragMessageCounter.Reset();
            Debug.Log($"_dragMessageCounter  {_dragMessageCounter}");
            var newPose = await _blazorApi.SendMessageWithResponse<BlockPoseChangingMessage, PoseChangeResponse>(
                new BlockPoseChangingMessage()
                {
                    BlockId = BlockId,
                    PositionX = newPosition.x,
                    PositionY = newPosition.y,
                    RotationZ = RotationZ,
                });

            if (cts.IsCancellationRequested)
            {
                return;
            }
            this.Position = new Vector2(newPose.NewPositionX, newPose.NewPositionY);
            this.RotationZ = newPose.NewRotationZ;

            UpdatePose();
            _dragMessageCounter.SetResult();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"OnBeginDrag  {BlockId}");
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag  {BlockId}");
            
            await _dragMessageCounter.Awaitable;
            _blazorApi.SendMessage(
                new BlockPoseChangedMessage()
                {
                    BlockId = BlockId,
                    PositionX = Position.x,
                    PositionY = Position.y,
                    RotationZ = RotationZ,
                });
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Debug.Log($"OnInitializePotentialDrag  {BlockId}");
            
            
            _dragPlane=new Plane(_backgroundPlane.transform.forward,eventData.pointerCurrentRaycast.worldPosition);
            
            var localHit= _backgroundPlane.transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);

            _dragOffset = localHit- transform.localPosition;
        }
    }
}