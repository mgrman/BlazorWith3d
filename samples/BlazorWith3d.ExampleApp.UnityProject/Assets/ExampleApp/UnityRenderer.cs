using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BlazorWith3d.ExampleApp.Client.Shared;

using UnityEngine;

using Random = UnityEngine.Random;

namespace ExampleApp
{
    public class UnityRenderer : MonoBehaviour, IBlocksOnGrid3DRenderer
    {
        [SerializeField] 
        private BlockController _templatePrefab;

        private readonly Dictionary<int, BlockController> _blocks = new();
        private readonly Dictionary<int, BlockController> _templates = new();
        private GameObject _templateRoot;

        private readonly List<IDisposable> _disposables=new ();
        private readonly List<IAsyncDisposable> _asyncDisposables=new ();

        [SerializeField]
        private Camera _camera;

        private IBlocksOnGrid3DController _eventHandler;

        private async Awaitable OnDestroy()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            foreach (var disposable in _asyncDisposables)
            {
                await disposable.DisposeAsync();
            }
        }

        // called by the controller signifying it can accept messages
        public ValueTask Initialize(IBlocksOnGrid3DController controller)
        {
            _eventHandler = controller;
            
            _templateRoot = new GameObject("BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;
            return new ValueTask();
        }

        public async ValueTask InitializeRenderer(RendererInitializationInfo msg)
        {
            Debug.Log($"BlazorControllerInitialized: ");
            _camera.transform.position = new Vector3(msg.RequestedCameraPosition.X, msg.RequestedCameraPosition.Y, -msg.RequestedCameraPosition.Z);
            _camera.transform.localRotation = Quaternion.Euler(msg.RequestedCameraRotation.ToUnity());
            
            
            _camera.backgroundColor = msg.BackgroundColor.ToUnity();
            _camera.fieldOfView = msg.RequestedCameraFoV;
            
            await _eventHandler.OnUnityAppInitialized(new UnityAppInitialized());
        }
        

        public ValueTask InvokeAddBlockTemplate(AddBlockTemplate msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");

            var meshGo = Instantiate(_templatePrefab, _templateRoot.transform);

            meshGo.Initialize(msg);

            _templates.Add(msg.TemplateId, meshGo);

            Debug.Log($"Added block template: {JsonUtility.ToJson(msg)}");
            return new ValueTask();
        }

        public ValueTask  InvokeRemoveBlockTemplate(RemoveBlockTemplate msg)
        {
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            
            GameObject.Destroy(_templates[msg.TemplateId].gameObject);
            _templates.Remove(msg.TemplateId);
            
            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
            return new ValueTask();
        }

        public ValueTask  InvokeAddBlockInstance(AddBlockInstance msg)
        {
            Debug.Log($"Adding block : {JsonUtility.ToJson(msg)}");
            var template = _templates[msg.TemplateId];


            var instance = template.CreateInstance(msg);
            _blocks.Add(msg.BlockId, instance);
            Debug.Log($"Added block : {JsonUtility.ToJson(msg)}");
            return new ValueTask();
        }

        public ValueTask  InvokeRemoveBlockInstance(RemoveBlockInstance msg)
        {
            Debug.Log($"Removing block: {JsonUtility.ToJson(msg)}");
            Destroy( _blocks[msg.BlockId].gameObject);
            _blocks.Remove(msg.BlockId);

            Debug.Log($"Removed block: {JsonUtility.ToJson(msg)}");
            return new ValueTask();
        }

        public ValueTask InvokeUpdateBlockInstance(int? blockId, PackableVector2 position, float rotationZ)
        {
            if (blockId == null || !_blocks.TryGetValue(blockId.Value, out var block))
            {
                return new ValueTask();
            }

            block.UpdatePose(position, rotationZ);
            return new ValueTask();
        }

        public async ValueTask InvokeTriggerTestToBlazor(TriggerTestToBlazor msg)
        {
            await Awaitable.WaitForSecondsAsync(1);

            var id = Random.Range(0, 1000);
            var response = await _eventHandler.OnTestToBlazor(new TestToBlazor(){Id = id});

            if (response.Id != id)
            {
                throw new InvalidOperationException();
            }
            Debug.Log("TriggerTestToBlazor is done");
        }

        public ValueTask<PerfCheck> InvokePerfCheck(PerfCheck msg)
        {
            return new ValueTask<PerfCheck>(msg);
        }

        public ValueTask<ScreenToWorldRayResponse>  InvokeRequestScreenToWorldRay(RequestScreenToWorldRay obj)
        {
            // convert to Unity screen coordinates
            var unityScreenPoint = new Vector3(obj.Screen.X, Screen.height - obj.Screen.Y, 0);
            
            var ray = Camera.main.ScreenPointToRay(unityScreenPoint);

            // convert ray to expected blazor world coordinate system
            ray = new UnityEngine.Ray(transform.worldToLocalMatrix.MultiplyPoint(ray.origin),
                transform.worldToLocalMatrix.MultiplyVector(ray.direction));

            return new ValueTask<ScreenToWorldRayResponse>(new ScreenToWorldRayResponse()
            {
                Ray = ray.ToNumerics()
            });
        }


        public ValueTask<RaycastResponse> InvokeRequestRaycast(RequestRaycast obj)
        {
            var ray = obj.Ray.ToUnity();

            // convert ray from blazor world coordinate system to unity
            ray = new UnityEngine.Ray(transform.localToWorldMatrix.MultiplyPoint(ray.origin),
                transform.localToWorldMatrix.MultiplyVector(ray.direction));
            
            var hitController = Physics.Raycast(ray, out RaycastHit hit)
                ? hit.collider.gameObject.GetComponentInParent<BlockController>()
                : null;
            if (hitController==null)
            {
                return new ValueTask<RaycastResponse> ( new RaycastResponse()
                {
                    HitBlockId = null, HitWorld = obj.Ray.Origin
                });
            }
            
            return new ValueTask<RaycastResponse> (  new RaycastResponse()
            {
                HitBlockId = hitController.BlockId, 
                // convert result to blazor world coordinate system
                HitWorld =  transform.worldToLocalMatrix.MultiplyPoint(hit.point).ToNumerics()
            });
        }
    }
}