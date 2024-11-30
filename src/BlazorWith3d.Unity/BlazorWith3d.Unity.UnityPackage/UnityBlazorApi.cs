using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using BlazorWith3d.Unity.Shared;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class UnityBlazorApi : IBinaryApi
    {
        private static readonly List<byte[]> messageBuffer = new();

        private static Action<byte[]> _onHandleReceivedMessages;


        public event Action<byte[]> OnMessage
        {
            add
            {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
                if (Application.isEditor|| Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    throw new NotImplementedException();
                }
#endif
                _onHandleReceivedMessages += value;
                if (messageBuffer.Any())
                {
                    foreach (var msg in messageBuffer)
                    {
                        Debug.Log($"replaying msg of {msg.Length} bytes as  onHandleReceivedMessages is set");
                        value(msg);
                    }
                    messageBuffer.Clear();
                }
            }
            remove
            {
                _onHandleReceivedMessages -= value;
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

            if (_onHandleReceivedMessages == null)
            {
                Debug.Log($"received msg of {bytes.Length} bytes but onHandleReceivedMessages is null");
                messageBuffer.Add(bytes);
            }
            else
            {
                Debug.Log($"received msg of {bytes.Length} bytes and invoked onHandleReceivedMessages");
                _onHandleReceivedMessages?.Invoke(bytes);
            }
        }

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(byte[] message, int size);

        [DllImport("__Internal")]
        private static extern void _ReadBytesBuffer(int id, byte[] array);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Action<int, int> instantiateByteArrayCallback);
    }
}