using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    public class BlazorApi : IBlazorApi
    {
        private static List<byte[]> messageBuffer = new ();

        private static Action<byte[]> _onHandleReceivedMessages;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void OnBeforeSplashScreen()
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            return;
#endif
            WebGLInput.captureAllKeyboardInput = false;

            Debug.Log($"On before BlazorApiUnity.InitializeApi");
            _InitializeApi(_InstantiateByteArray);

            Debug.Log($"On after BlazorApiUnity.InitializeApi");
            var bytes = Encoding.Unicode.GetBytes("UNITY_INITIALIZED");
            _SendMessageFromUnity(bytes,bytes.Length);
        }

        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _InstantiateByteArray(int size, int id)
        {
            Debug.Log($"_InstantiateByteArray({size},{id})");
            var bytes = new byte[size];
            
            _ReadBytesBuffer(id, bytes);
            Debug.Log($"_ReadBytesBuffer({id},bytes)");
            
            Debug.Log($"Received message ({string.Join(", ",bytes)})");
            if (_onHandleReceivedMessages == null)
            {
                messageBuffer.Add(bytes);
            }
            else
            {
                _onHandleReceivedMessages?.Invoke(bytes);
            }
        }


        public Action<byte[]> OnHandleReceivedMessages
        {
            get => _onHandleReceivedMessages;
            set
            {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
                throw new NotImplementedException();
#endif
                _onHandleReceivedMessages = value;
                if (value != null)
                {
                    foreach (var msg in messageBuffer)
                    {
                        value(msg);
                    }
                    messageBuffer.Clear();
                }
            }
        }

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(byte[] message, int size);

        [DllImport("__Internal")]
        private static extern void _ReadBytesBuffer(int id, byte[] array);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Action<int, int> instantiateByteArrayCallback);

        public void SendMessageFromUnity(byte[] bytes)
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            throw new NotImplementedException();
#endif
            _SendMessageFromUnity(bytes,bytes.Length);
        }
    }
}