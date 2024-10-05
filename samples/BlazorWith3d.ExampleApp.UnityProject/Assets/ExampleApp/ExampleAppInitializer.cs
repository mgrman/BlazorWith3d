using System;
using System.Collections.Generic;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using BlazorWith3d.Unity.Shared;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ExampleApp
{
    public class ExampleAppInitializer :MonoBehaviour
    {
        private readonly Dictionary<int, (GameObject template, Vector3 size)> _templates=new ();
        private readonly Dictionary<int, GameObject > _blocks=new ();
        private GameObject _templateRoot;

        public void Start()
        {

            _templateRoot=new GameObject($"BlockTemplateRoot");
            _templateRoot.SetActive(false);
            _templateRoot.transform.parent = transform;

            var typedApi = new TypedMessageBlazorApi();
            
            typedApi.AddMessageProcessCallback<AddBlockTemplateMessage>(OnAddBlockTemplateMessage);
            typedApi.AddMessageProcessCallback<RemoveBlockTemplateMessage>(OnRemoveBlockTemplateMessage);
            typedApi.AddMessageProcessCallback<AddBlockInstanceMessage>(OnAddBlockInstanceMessage);
            typedApi.AddMessageProcessCallback<RemoveBlockMessage>(OnRemoveBlockMessage);

            typedApi.SendMessage(new AppInitialized());
            
            #if UNITY_EDITOR
            OnAddBlockTemplateMessage(new AddBlockTemplateMessage()
            {
                TemplateId = 0,
                SizeX = 1, SizeY = 1, SizeZ = 0, VisualsUri = null
            });
            OnAddBlockInstanceMessage(new AddBlockInstanceMessage(){ BlockId = 0, TemplateId = 0});
            OnRemoveBlockMessage(new RemoveBlockMessage() { BlockId = 0 });
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
        
        private void OnAddBlockTemplateMessage(AddBlockTemplateMessage msg)
        {
            Debug.Log($"Adding block template: {JsonUtility.ToJson(msg)}");
            var templateGo=new GameObject($"BlockTemplate_{msg.TemplateId}");
            templateGo.transform.SetParent(_templateRoot.transform);
            
            
            var meshGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGo.transform.SetParent(templateGo.transform);
            meshGo.transform.localScale=new Vector3(msg.SizeX, msg.SizeY, msg.SizeZ);
            meshGo.transform.localPosition=new Vector3(0, msg.SizeY/2,0);
            
            
            _templates.Add(msg.TemplateId, (meshGo,meshGo.transform.localScale));

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
            
             var blockGo=GameObject.Instantiate(template.template, new Vector3(msg.PositionX, msg.PositionY,0), Quaternion.Euler(0,0,msg.RotationZ),  transform  );
            
             _blocks.Add(msg.BlockId,blockGo );
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