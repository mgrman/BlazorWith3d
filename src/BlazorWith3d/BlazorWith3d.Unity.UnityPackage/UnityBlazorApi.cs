using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using BlazorWith3d.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class UnityBlazorApi : IBinaryApi
    {
        private static readonly ReceiveMessageBuffer _receiveMessageBuffer = new();



        public Action<byte[]>? MainMessageHandler
        {
            get => _receiveMessageBuffer.MainMessageHandler;
            set
            {
                
#if !(UNITY_WEBGL && !UNITY_EDITOR)
                if (Application.isEditor|| Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    throw new NotImplementedException();
                }
#endif
                _receiveMessageBuffer.MainMessageHandler = value;
            }
        }
        

        public ValueTask SendMessage(byte[] bytes)
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (Application.isEditor|| Application.platform != RuntimePlatform.WebGLPlayer)
            {
                throw new NotImplementedException();
            }
#endif
            _SendMessageFromUnity(bytes, bytes.Length);
            return new ValueTask();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashScreen()
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (Application.isEditor|| Application.platform != RuntimePlatform.WebGLPlayer)
            {
                return;
            }
#endif
            WebGLInput.captureAllKeyboardInput = false;

            Debug.Log("On before BlazorApiUnity.InitializeApi");
            _InitializeApi(_InstantiateByteArray);
        }

        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _InstantiateByteArray(int size, int id)
        {
            var bytes = new byte[size];

            _ReadBytesBuffer(id, bytes);

            _receiveMessageBuffer.InvokeMessage(bytes);
        }

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(byte[] message, int size);

        [DllImport("__Internal")]
        private static extern void _ReadBytesBuffer(int id, byte[] array);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Action<int, int> instantiateByteArrayCallback);
    }
}