#if UNITY_EDITOR
using System;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace ExampleApp
{
    public class DragChangingSimulatorHandler : MonoBehaviour, IBlazorSimulatorMessageHandler
    {
        public Type MessageType => typeof(BlockPoseChanging);
        public I3DAppObjectApi ObjectApi { get; set; }

        public async void HandleMessage(object messageObject)
        {
            var message = (BlockPoseChanging)messageObject;

            await ObjectApi.InvokeMessageObject(new BlockPoseChangeValidated()
            {
                BlockId = message.BlockId,
                IsValid = true,
                NewPositionX = message.PositionX,
                NewPositionY = (float)Math.Round(message.PositionY, 0),
                NewRotationZ = message.RotationZ,
                ChangingRequestId = message.ChangingRequestId
            });
        }
    }
}
#endif