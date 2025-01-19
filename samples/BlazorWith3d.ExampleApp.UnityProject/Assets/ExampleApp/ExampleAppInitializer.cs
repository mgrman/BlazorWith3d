using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Shared;
using BlazorWith3d.Unity;
using UnityEngine;

using Random = UnityEngine.Random;


namespace ExampleApp
{
    public class ExampleAppInitializer : MonoBehaviour, IBlocksOnGrid3DRenderer
    {
        public static Uri HostUrl = null;
        
        [SerializeField] private BlockController _templatePrefab;

        private readonly Dictionary<int, BlockController> _blocks = new();
        private readonly Dictionary<int, BlockController> _templates = new();
        private IBlocksOnGrid3DController _appApi;
        private GameObject _templateRoot;

        private List<IDisposable> _disposables=new List<IDisposable>();
        private List<IAsyncDisposable> _asyncDisposables=new List<IAsyncDisposable>();


        [Tooltip("If none is set, will use simulator")] 
        [SerializeField]
        private string _backendWebsocketUrl = "ws://localhost:5292/debug-relay-unity-ws";


        public async void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
            #endif
            Debug.Log($"cmdArgs: {string.Join(" ",Environment.GetCommandLineArgs())}");
            
            
            _templateRoot = new GameObject("BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;
            

#if UNITY_EDITOR

            if (string.IsNullOrEmpty(_backendWebsocketUrl))
            {
                throw new InvalidOperationException(
                    "_backendWebsocketUrl is not set! It must be set to run in Editor!");
            }
            else
            {
                var relay = new BlazorWebSocketRelay(_backendWebsocketUrl);

                var imageCapturer = new CameraImageStreamer(relay);
                _disposables.Add(imageCapturer);
                
                _asyncDisposables.Add(relay);
                var blazorApi = relay;
                _appApi = new BlocksOnGrid3DController_BinaryApiWithResponse(new BinaryApiWithResponseOverBinaryApi(blazorApi), new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
                
                var uriBuilder = new UriBuilder(_backendWebsocketUrl);
                uriBuilder.Scheme="http";
                uriBuilder.Path = "";
                HostUrl = uriBuilder.Uri;
            }
#else
            HostUrl = new Uri(Application.absoluteURL);
            var blazorApi = UnityBlazorApi.Singleton;

            if(Environment.GetCommandLineArgs().Contains("BinaryApiWithResponse", StringComparer.OrdinalIgnoreCase)){
                _appApi = new BlocksOnGrid3DController_BinaryApiWithResponse(blazorApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
            }
            else{
                _appApi = new BlocksOnGrid3DController_BinaryApiWithResponse(new BinaryApiWithResponseOverBinaryApi(blazorApi), new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());

            }
#endif
            Console.WriteLine($"{Screen.width},{Screen.height}");
            _appApi.SetRenderer(this);
            
            
                
#if !UNITY_EDITOR
            UnityBlazorApi.InitializeWebGLInterop();
#endif
            await _appApi.OnUnityAppInitialized(new UnityAppInitialized());
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

        public async ValueTask InvokeBlazorControllerInitialized(BlazorControllerInitialized _)
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


        public async ValueTask InvokeAddBlockTemplate(AddBlockTemplate msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");


            var meshGo = Instantiate(_templatePrefab, _templateRoot.transform);

            meshGo.Initialize(msg);

            _templates.Add(msg.TemplateId, meshGo);

            Debug.Log($"Added block template: {JsonUtility.ToJson(msg)}");
        }

        public async ValueTask  InvokeRemoveBlockTemplate(RemoveBlockTemplate msg)
        {
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            
            GameObject.Destroy(_templates[msg.TemplateId].gameObject);
            _templates.Remove(msg.TemplateId);
            
            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
        }

        public async ValueTask  InvokeAddBlockInstance(AddBlockInstance msg)
        {
            Debug.Log($"Adding block : {JsonUtility.ToJson(msg)}");
            var template = _templates[msg.TemplateId];


            var instance = template.CreateInstance(msg);
            _blocks.Add(msg.BlockId, instance);
            Debug.Log($"Added block : {JsonUtility.ToJson(msg)}");
        }

        public async ValueTask  InvokeRemoveBlockInstance(RemoveBlockInstance msg)
        {
            Debug.Log($"Removing block: {JsonUtility.ToJson(msg)}");
            Destroy( _blocks[msg.BlockId].gameObject);
            _blocks.Remove(msg.BlockId);

            Debug.Log($"Removed block: {JsonUtility.ToJson(msg)}");
        }

        public async ValueTask InvokeUpdateBlockInstance(int? blockId, PackableVector2 position, float rotationZ)
        {
            if (blockId == null || !_blocks.TryGetValue(blockId.Value, out var block))
            {
                return;
            }

            block.UpdatePose(position, rotationZ);
        }

        public async ValueTask InvokeTriggerTestToBlazor(TriggerTestToBlazor msg)
        {
            await Awaitable.WaitForSecondsAsync(1);

            var id = Random.Range(0, 1000);
            var response = await _appApi.OnTestToBlazor(new TestToBlazor(){Id = id});

            if (response.Id != id)
            {
                throw new InvalidOperationException();
            }
            Debug.Log("TriggerTestToBlazor is done");
        }

        public async ValueTask<PerfCheck> InvokePerfCheck(PerfCheck msg)
        {
            return msg;
        }

        public async ValueTask<ScreenToWorldRayResponse>  InvokeRequestScreenToWorldRay(RequestScreenToWorldRay obj)
        {
            // convert to Unity screen coordinates
            var unityScreenPoint = new Vector3(obj.Screen.X, Screen.height - obj.Screen.Y, 0);
            
            var ray = Camera.main.ScreenPointToRay(unityScreenPoint);

            // convert ray to expected blazor world coordinate system
            ray = new UnityEngine.Ray(transform.worldToLocalMatrix.MultiplyPoint(ray.origin),
                transform.worldToLocalMatrix.MultiplyVector(ray.direction));

            return new ScreenToWorldRayResponse()
            {
                Ray = ray.ToNumerics()
            };
        }

        public void SetController(IBlocksOnGrid3DController controller)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<RaycastResponse> InvokeRequestRaycast(RequestRaycast obj)
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
              return  new RaycastResponse()
                {
                    HitBlockId = null, HitWorld = obj.Ray.Origin
                };
            }
            
            return new RaycastResponse()
            {
                HitBlockId = hitController.BlockId, 
                // convert result to blazor world coordinate system
                HitWorld =  transform.worldToLocalMatrix.MultiplyPoint(hit.point).ToNumerics()
            };
        }
    }

#if UNITY_EDITOR

    public class CameraImageStreamer:IDisposable
    {
        private readonly BlazorWebSocketRelay _relay;
        private readonly CancellationTokenSource _cts;
        private readonly RenderTexture _renderTexture;
        private readonly Texture2D _screenshot;

        public CameraImageStreamer(BlazorWebSocketRelay relay)
        {
            _relay = relay;
            _cts = new CancellationTokenSource();
            _renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            
            _screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            Stream();

        }

        private async Awaitable Stream()
        {
            while (!_cts.IsCancellationRequested && Application.isPlaying)  
            {
                try
                {
                    await Awaitable.NextFrameAsync();

                    if (!_relay.IsConnected)
                    {
                        continue;
                    }

                    var cam = Camera.main;
                    cam.targetTexture = _renderTexture;
                    cam.Render();
                    cam.targetTexture = null;

                    RenderTexture.active = _renderTexture;
                    _screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                    _screenshot.Apply();
                    RenderTexture.active = null;

                    byte[] bytes = _screenshot.EncodeToJPG();

                    await _relay.UpdateScreen(bytes);

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
#endif
}