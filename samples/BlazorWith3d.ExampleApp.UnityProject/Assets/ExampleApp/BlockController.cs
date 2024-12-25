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

            _cubePlaceholderVisuals.transform.localScale = new Vector3(_template.Size.X, _template.Size.Y, _template.Size.Z);
            _cubePlaceholderVisuals.transform.localPosition = new Vector3(0, 0, -_template.Size.Z / 2);
        }

        public BlockController CreateInstance(AddBlockInstance msg)
        {
            var blockGo = Instantiate(this, this.transform.parent.parent);
            blockGo._template = this._template;

            blockGo.InitializeInstance(msg);

            return blockGo;
        }

        private void InitializeInstance(AddBlockInstance msg)
        {
            _instance = msg;

            UpdatePose();
        }

        public void UpdatePose(UpdateBlockInstance newPose)
        {
            _instance.Position.X=newPose.Position.X;
            _instance.Position.Y = newPose.Position.Y;
            _instance.RotationZ = newPose.RotationZ;

            UpdatePose();
        }

        private void UpdatePose()
        {
            transform.localPosition = new Vector3(_instance.Position.X, _instance.Position.Y, 0);
            transform.localRotation = Quaternion.Euler(0, 0, _instance.RotationZ);
        }
    }
}