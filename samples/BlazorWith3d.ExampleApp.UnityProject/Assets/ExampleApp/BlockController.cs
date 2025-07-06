using System;

using BlazorWith3d.ExampleApp.Client.Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ExampleApp
{
    public class BlockController : MonoBehaviour
    {
        private static int _validationCounter;

        [SerializeField] 
        private GameObject _cubePlaceholderVisuals;

        private AddBlockTemplate _template;
        private AddBlockInstance _instance;

        public int BlockId =>_instance.BlockId;

        public void Initialize(AddBlockTemplate msg)
        {
            _template = msg;

            _cubePlaceholderVisuals.transform.localScale =
                new Vector3(_template.Size.X, _template.Size.Y, _template.Size.Z);
            _cubePlaceholderVisuals.transform.localPosition = new Vector3(0, 0, _template.Size.Z / 2);
            _cubePlaceholderVisuals.transform.localRotation =Quaternion.Euler(0,180,0);

            if (!string.IsNullOrEmpty(msg.Visuals3dUri) )
            {
                _cubePlaceholderVisuals.GetComponent<MeshRenderer>().enabled = false;
                
                var gltf = gameObject.AddComponent<GLTFast.GltfAsset>();

                var absUrl =  new Uri(ExampleAppInitializer.HostUrl, msg.Visuals3dUri).AbsoluteUri;
                gltf.Url=absUrl;
                gltf.Load(absUrl);
            }
        }

        public BlockController CreateInstance(AddBlockInstance msg)
        {
            var blockGo = Instantiate(this.gameObject, this.transform.parent.parent).GetComponent<BlockController>();
            blockGo._template = this._template;

            blockGo.InitializeInstance(msg);

            return blockGo;
        }

        private void InitializeInstance(AddBlockInstance msg)
        {
            _instance = msg;

            UpdatePose();
        }

        public void UpdatePose(PackableVector2 position,float rotationZ)
        {
            _instance.Position = position;
            _instance.RotationZ = rotationZ;

            UpdatePose();
        }

        private void UpdatePose()
        {
            transform.localPosition = new Vector3(_instance.Position.X, _instance.Position.Y, 0);
            transform.localRotation = Quaternion.Euler(0, 180, _instance.RotationZ);
        }
    }
}