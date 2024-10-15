#if UNITY_EDITOR
using System;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using UnityEngine;

namespace ExampleApp
{
    public class DragChangingSimulatorHandler: MonoBehaviour,IBlazorSimulatorMessageHandler 
    {
        public Type MessageType => typeof(BlockPoseChangingMessage);
        public TypedUnityApi UnityApi { get; set; }

        public void HandleMessage(object messageObject)
        {
            var message=(BlockPoseChangingMessage)messageObject;

            UnityApi.SendMessage(new BlockPoseChangingResponse()
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