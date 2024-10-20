using System;
using System.Collections.Generic;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace ExampleApp
{
    public class ExampleAppInitializer : MonoBehaviour
    {
        [SerializeField] private BlockController _templatePrefab;

        [SerializeField] private GameObject _backgroundPlane;

        private readonly Dictionary<int, BlockController> _blocks = new();
        private readonly Dictionary<int, BlockController> _templates = new();
        private IBlocksOnGridUnityApi _appApi;
        private IBlazorApi _blazorApi;
        private GameObject _templateRoot;

        private TypedBlazorApi _typedApi;
        
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
                var simulatorApi = new SimulatorApi();
                var simulatorTypedApi = new UnityTypedUnityApi(simulatorApi);
                var simulator = gameObject.AddComponent<BlazorSimulator>();
                gameObject.AddComponent<DragChangingSimulatorHandler>();
                simulator.Initialize(simulatorTypedApi, typeof(AppInitialized).Assembly);
                _blazorApi = simulatorApi;
            }
            else
            {
                var relay = new BlazorWebSocketRelay(_backendWebsocketUrl);
                _asyncDisposables.Add(relay);
                _blazorApi = relay;
            }
#else
            _blazorApi = new UnityBlazorApi();
#endif
            _typedApi = new UnityTypedBlazorApi(_blazorApi);

            _appApi = new BlocksOnGridUnityApi(_typedApi);

            _appApi.AppInitialized(new AppInitialized());
            _appApi.AddBlockTemplate += OnAddBlockTemplateMessage;
            _appApi.RemoveBlockTemplate += OnRemoveBlockTemplateMessage;
            _appApi.AddBlockInstance += OnAddBlockInstanceMessage;
            _appApi.RemoveBlock += OnRemoveBlockMessage;
            _appApi.BlockPoseChangingResponse += OnBlockPoseChangingResponse;
            _appApi.ControllerInitialized += OnControllerInitialized;
            
            _appApi.PerfCheckRequest += request =>
            {
                _appApi.PerfCheckResponse(new PerfCheckResponse
                {
                    Id = request.Id,
                    Aaa = request.Aaa,
                    Bbb = request.Bbb,
                    Ccc = request.Ccc,
                    Ddd = request.Ddd
                });
            };
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

        private void OnControllerInitialized(ControllerInitializedRequest _)
        {
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

        private void OnBlockPoseChangingResponse(BlockPoseChangingResponse obj)
        {
            _blocks[obj.BlockId].OnBlockPoseChangingResponse(obj);
        }

        private void OnAddBlockTemplateMessage(AddBlockTemplateMessage msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");


            var meshGo = Instantiate(_templatePrefab, _templateRoot.transform);

            meshGo.Initialize(msg, gameObject, _typedApi);

            _templates.Add(msg.TemplateId, meshGo);

            Debug.Log($"Added block template: {JsonUtility.ToJson(msg)}");
        }

        private void OnRemoveBlockTemplateMessage(RemoveBlockTemplateMessage msg)
        {
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            
            GameObject.Destroy(_templates[msg.TemplateId].gameObject);
            _templates.Remove(msg.TemplateId);
            
            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
        }

        private void OnAddBlockInstanceMessage(AddBlockInstanceMessage msg)
        {
            Debug.Log($"Adding block : {JsonUtility.ToJson(msg)}");
            var template = _templates[msg.TemplateId];


            var instance = template.CreateInstance(msg);
            _blocks.Add(msg.BlockId, instance);
            Debug.Log($"Added block : {JsonUtility.ToJson(msg)}");
        }

        private void OnRemoveBlockMessage(RemoveBlockMessage msg)
        {
            Debug.Log($"Removing block: {JsonUtility.ToJson(msg)}");
            Destroy( _blocks[msg.BlockId].gameObject);
            _blocks.Remove(msg.BlockId);

            Debug.Log($"Removed block: {JsonUtility.ToJson(msg)}");
        }
    }
}