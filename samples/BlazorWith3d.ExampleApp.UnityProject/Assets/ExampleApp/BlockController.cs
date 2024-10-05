using System;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExampleApp
{
    public class BlockController:MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        
        [SerializeField]
        private GameObject _cubePlaceholderVisuals;

        private AddBlockTemplateMessage _blockTemplateInfo;
        private AddBlockInstanceMessage _blockInstanceInfo;
        
        
        private GameObject _backgroundPlane;
        
        
        private Plane _dragPlane;
        private Vector3 _dragOffset;

        public void Initialize(AddBlockTemplateMessage msg, GameObject backgroundPlane)
        {
            _blockTemplateInfo = msg;
            _backgroundPlane=backgroundPlane;
            
            _cubePlaceholderVisuals.transform.localScale=new Vector3(msg.SizeX, msg.SizeY, msg.SizeZ);
            _cubePlaceholderVisuals.transform.localPosition=new Vector3(0, msg.SizeY/2,0);
        }
        
        public BlockController CreateInstance(AddBlockInstanceMessage msg)
        {
            var blockGo=GameObject.Instantiate(this, _backgroundPlane.transform  );
            blockGo.transform.localPosition= new Vector3(msg.PositionX, msg.PositionY,0);
            blockGo.transform.localRotation= Quaternion.Euler(0, 0, msg.RotationZ); 
            blockGo._blockTemplateInfo = _blockTemplateInfo;
            blockGo._backgroundPlane = _backgroundPlane;
            blockGo._blockInstanceInfo = msg;
            
            return blockGo;
        }

        public void OnDrag(PointerEventData eventData)
        {

            var ray = Camera.main.ScreenPointToRay(eventData.pointerCurrentRaycast.screenPosition);

            if (!_dragPlane.Raycast(ray, out var distance))
            {
                return;
            }
            
            var dragPlaneHit=_backgroundPlane.transform.InverseTransformPoint(ray.GetPoint(distance));

            transform.localPosition=dragPlaneHit-_dragOffset;
            
            Debug.Log($"OnDrag  {_blockInstanceInfo.BlockId}");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"OnBeginDrag  {_blockInstanceInfo.BlockId}");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag  {_blockInstanceInfo.BlockId}");
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Debug.Log($"OnInitializePotentialDrag  {_blockInstanceInfo.BlockId}");
            
            
            _dragPlane=new Plane(_backgroundPlane.transform.forward,eventData.pointerCurrentRaycast.worldPosition);
            
            var localHit= _backgroundPlane.transform.InverseTransformPoint(eventData.pointerCurrentRaycast.worldPosition);

            _dragOffset = localHit- transform.localPosition;

        }
    }
}