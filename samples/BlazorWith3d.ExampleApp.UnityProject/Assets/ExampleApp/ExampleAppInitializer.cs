using System;
using System.Collections.Generic;
using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Unity;
using UnityEngine;


namespace ExampleApp
{
    public class ExampleAppInitializer : MonoBehaviour
    {
        [SerializeField] private BlockController _templatePrefab;

        private readonly Dictionary<int, BlockController> _blocks = new();
        private readonly Dictionary<int, BlockController> _templates = new();
        private IBlocksOnGridUnityApi _appApi;
        private GameObject _templateRoot;

        private List<IDisposable> _disposables=new List<IDisposable>();
        private List<IAsyncDisposable> _asyncDisposables=new List<IAsyncDisposable>();


        [Tooltip("If none is set, will use simulator")] 
        [SerializeField]
        private string _backendWebsocketUrl = "ws://localhost:5292/debug-relay-unity-ws";


        public async void Start()
        {
            _templateRoot = new GameObject("BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;

#if UNITY_EDITOR

            if (string.IsNullOrEmpty(_backendWebsocketUrl))
            {
                var simulatorProxy = new SimulatorProxy();
                _appApi = new BlocksOnGridUnityApi(simulatorProxy.BlazorApi);
                var blazorApp = new BlocksOnGrid3DApp(simulatorProxy.UnityApi);
                var simulator = gameObject.AddComponent<BlazorSimulator>();
                simulator.Initialize(blazorApp);
            }
            else
            {
                var relay = new BlazorWebSocketRelay(_backendWebsocketUrl);
                _asyncDisposables.Add(relay);
                var blazorApi = relay;
                _appApi = new BlocksOnGridUnityApi(blazorApi);
            }
#else
            var blazorApi = new UnityBlazorApi();
            _appApi = new BlocksOnGridUnityApi(blazorApi);
#endif
            Console.WriteLine($"{Screen.width},{Screen.height}");
            await _appApi.InvokeUnityAppInitialized(new UnityAppInitialized()
            {
            });
            _appApi.OnBlazorControllerInitialized += OnControllerInitialized;
            _appApi.OnAddBlockTemplate += OnAddBlockTemplateMessage;
            _appApi.OnRemoveBlockTemplate += OnRemoveBlockTemplateMessage;
            _appApi.OnAddBlockInstance += OnAddBlockInstanceMessage;
            _appApi.OnRemoveBlockInstance += OnRemoveBlockMessage;
            _appApi.OnUpdateBlockInstance += OnUpdateBlockInstance;
            _appApi.OnRequestRaycast += OnRequestRaycast;
            _appApi.OnRequestScreenToWorldRay += OnRequestScreenToWorldRay;

            _appApi.OnPerfCheck += request =>
            {
                _appApi.InvokePerfCheck(new PerfCheck
                {
                    Id = request.Id,
                    Aaa = request.Aaa,
                    Bbb = request.Bbb,
                    Ccc = request.Ccc,
                    Ddd = request.Ddd
                });
            };
            _appApi.StartProcessingMessages();
        }

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

        private void OnControllerInitialized(BlazorControllerInitialized _)
        {
            Debug.Log($"BlazorControllerInitialized: ");
            foreach (var block in _templates.Values)
            {
                GameObject.Destroy(block.gameObject);
            }

            _templates.Clear();
            
            foreach (var block in _blocks.Values)
            {
                GameObject.Destroy(block.gameObject);
            }

            _blocks.Clear();
        }


        private void OnAddBlockTemplateMessage(AddBlockTemplate msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");


            var meshGo = Instantiate(_templatePrefab, _templateRoot.transform);

            meshGo.Initialize(msg);

            _templates.Add(msg.TemplateId, meshGo);

            Debug.Log($"Added block template: {JsonUtility.ToJson(msg)}");
        }

        private void OnRemoveBlockTemplateMessage(RemoveBlockTemplate msg)
        {
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            
            GameObject.Destroy(_templates[msg.TemplateId].gameObject);
            _templates.Remove(msg.TemplateId);
            
            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
        }

        private void OnAddBlockInstanceMessage(AddBlockInstance msg)
        {
            Debug.Log($"Adding block : {JsonUtility.ToJson(msg)}");
            var template = _templates[msg.TemplateId];


            var instance = template.CreateInstance(msg);
            _blocks.Add(msg.BlockId, instance);
            Debug.Log($"Added block : {JsonUtility.ToJson(msg)}");
        }

        private void OnRemoveBlockMessage(RemoveBlockInstance msg)
        {
            Debug.Log($"Removing block: {JsonUtility.ToJson(msg)}");
            Destroy( _blocks[msg.BlockId].gameObject);
            _blocks.Remove(msg.BlockId);

            Debug.Log($"Removed block: {JsonUtility.ToJson(msg)}");
        }

        private void OnUpdateBlockInstance(UpdateBlockInstance obj)
        {
            _blocks[obj.BlockId].UpdatePose(obj);
        }

        private async void OnRequestScreenToWorldRay(RequestScreenToWorldRay obj)
        {
            // convert to Unity screen coordinates
            var unityScreenPoint = new Vector3(obj.Screen.X, Screen.height - obj.Screen.Y, 0);
            
            var ray = Camera.main.ScreenPointToRay(unityScreenPoint);

            // convert ray to expected blazor world coordinate system
            ray = new UnityEngine.Ray(transform.worldToLocalMatrix.MultiplyPoint(ray.origin),
                transform.worldToLocalMatrix.MultiplyVector(ray.direction));

            await _appApi.InvokeScreenToWorldRayResponse(new ScreenToWorldRayResponse()
            {
                RequestId = obj.RequestId, Ray = ray.ToNumerics()
            });
            return;
        }

        private async void OnRequestRaycast(RequestRaycast obj)
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
                await _appApi.InvokeRaycastResponse(new RaycastResponse()
                {
                    RequestId = obj.RequestId,
                    HitBlockId = null, HitWorld = obj.Ray.Origin
                });
                return;
            }
            
            await _appApi.InvokeRaycastResponse(new RaycastResponse()
            {
                RequestId = obj.RequestId,
                HitBlockId = hitController.BlockId, 
                // convert result to blazor world coordinate system
                HitWorld =  transform.worldToLocalMatrix.MultiplyPoint(hit.point).ToNumerics()
            });
        }
    }
}