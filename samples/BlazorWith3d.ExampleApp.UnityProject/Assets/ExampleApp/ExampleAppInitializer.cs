using System;
using System.Collections.Generic;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using BlazorWith3d.Unity.Shared;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ExampleApp
{
    public class ExampleAppInitializer :MonoBehaviour
    {
        private readonly Dictionary<int, BlockController> _templates=new ();
        private readonly Dictionary<int, BlockController > _blocks=new ();
        private GameObject _templateRoot;
        
        [SerializeField]
        private BlockController _templatePrefab;
        
        [SerializeField]
        private GameObject _backgroundPlane;

        private TypedMessageBlazorApi _typedApi;

        public void Start()
        {
            _templateRoot=new GameObject($"BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;

            _typedApi = new TypedMessageBlazorApi();
            
            _typedApi.AddMessageProcessCallback<AddBlockTemplateMessage>(OnAddBlockTemplateMessage);
            _typedApi.AddMessageProcessCallback<RemoveBlockTemplateMessage>(OnRemoveBlockTemplateMessage);
            _typedApi.AddMessageProcessCallback<AddBlockInstanceMessage>(OnAddBlockInstanceMessage);
            _typedApi.AddMessageProcessCallback<RemoveBlockMessage>(OnRemoveBlockMessage);

            _typedApi.SendMessage(new AppInitialized());
            
            #if UNITY_EDITOR
            OnAddBlockTemplateMessage(new AddBlockTemplateMessage()
            {
                TemplateId = 0,
                SizeX = 1, SizeY = 1, SizeZ = 1, VisualsUri = null
            });
            OnAddBlockInstanceMessage(new AddBlockInstanceMessage(){ BlockId = 0, TemplateId = 0, PositionX = 0, PositionY = 0, RotationZ = 0});
            //OnRemoveBlockMessage(new RemoveBlockMessage() { BlockId = 0 });
            //
            // OnAddBlockTemplateMessage(new AddBlockTemplateMessage()
            // {
            //     TemplateId = 1,
            //     SizeX = 0, SizeY = 0, SizeZ = 0, VisualsUri = null
            // });

            // TypedMessageBlazorApi.SimulateMessage(
            //     @"AddBlockTemplateMessage;{""TemplateId"":0,""SizeX"":1.0,""SizeY"":2.0,""SizeZ"":3.0}");
            // TypedMessageBlazorApi.SimulateMessage(
            //     @"AddBlockTemplateMessage;{""TemplateId"":0,""SizeX"":1.0,""SizeY"":2.0,""SizeZ"":3.0}");
            #endif
            
            
            
        }

        private void Update()
        {
          //  Debug.Log(EventSystem.current.currentSelectedGameObject);
        }

        private void OnAddBlockTemplateMessage(AddBlockTemplateMessage msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");
            
            
            var meshGo = GameObject.Instantiate(_templatePrefab, _templateRoot.transform);
            
            meshGo.Initialize(msg,gameObject, _typedApi);
            
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
            var template=_templates[msg.TemplateId];


            var instance=template.CreateInstance(msg);
             _blocks.Add(msg.BlockId,instance );
             Debug.Log($"Added block : {JsonUtility.ToJson(msg)}");
        }

        private void OnRemoveBlockMessage(RemoveBlockMessage msg)
        {
            Debug.Log($"Removing block template: {JsonUtility.ToJson(msg)}");
            var blockGo=_blocks[msg.BlockId];
            GameObject.Destroy(blockGo);
            _blocks.Remove(msg.BlockId);
            
            Debug.Log($"Removed block template: {JsonUtility.ToJson(msg)}");
        }
    }
}