#if UNITY_EDITOR
using System;
using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using UnityEngine;

namespace ExampleApp
{
    public class DragChangingSimulatorHandler: MonoBehaviour,IBlazorSimulatorMessageWithResponseHandler 
    {
        public Type MessageType => typeof(BlockPoseChangingMessage);
        public Type ResponseType => typeof(PoseChangeResponse);
        public object HandleMessage(object messageObject)
        {
            var message=(BlockPoseChangingMessage)messageObject;

            return new PoseChangeResponse()
            {
                IsValid = true,
                NewPositionX = message.PositionX,
                NewPositionY = (float)Math.Round(message.PositionY, 0),
                NewRotationZ = message.RotationZ,
            };
        }
    }
}
#endif