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
        private MyBlazorApi _appApi;
        private IBlazorApi _blazorApi;
        private GameObject _templateRoot;

        private TypedBlazorApi _typedApi;

        public void Start()
        {
            _templateRoot = new GameObject("BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;

#if UNITY_EDITOR
            var simulatorApi = new SimulatorApi();
            var simulatorTypedApi = new UnityTypedUnityApi(simulatorApi);
            var simulator = gameObject.AddComponent<BlazorSimulator>();
            gameObject.AddComponent<DragChangingSimulatorHandler>();
            simulator.Initialize(simulatorTypedApi, typeof(AppInitialized).Assembly);
            _blazorApi = simulatorApi;
#else
            _blazorApi = new UnityBlazorApi();
#endif
            _typedApi = new UnityTypedBlazorApi(_blazorApi);

            _appApi = new MyBlazorApi(_typedApi);

            _typedApi.AddMessageProcessCallback<AddBlockTemplateMessage>(OnAddBlockTemplateMessage);
            _typedApi.AddMessageProcessCallback<RemoveBlockTemplateMessage>(OnRemoveBlockTemplateMessage);
            _typedApi.AddMessageProcessCallback<AddBlockInstanceMessage>(OnAddBlockInstanceMessage);
            _typedApi.AddMessageProcessCallback<RemoveBlockMessage>(OnRemoveBlockMessage);
            _typedApi.AddMessageProcessCallback<BlockPoseChangingResponse>(OnBlockPoseChangingResponse);

            _typedApi.SendMessage(new AppInitialized());

#if UNITY_EDITOR

            simulatorTypedApi.SendMessage(new AddBlockTemplateMessage
            {
                TemplateId = 0,
                SizeX = 1, SizeY = 1, SizeZ = 1, VisualsUri = null
            });
            simulatorTypedApi.SendMessage(new AddBlockInstanceMessage
                { BlockId = 0, TemplateId = 0, PositionX = 0, PositionY = 0, RotationZ = 0 });
#endif

            _appApi.OnPerfCheckRequest += request =>
            {
                _appApi.InvokePerfCheckResponse(new PerfCheckResponse
                {
                    Id = request.Id,
                    Aaa = request.Aaa,
                    Bbb = request.Bbb,
                    Ccc = request.Ccc,
                    Ddd = request.Ddd
                });
            };
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
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            var blockGo = _blocks[msg.BlockId];
            Destroy(blockGo);
            _blocks.Remove(msg.BlockId);

            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
        }
    }
}