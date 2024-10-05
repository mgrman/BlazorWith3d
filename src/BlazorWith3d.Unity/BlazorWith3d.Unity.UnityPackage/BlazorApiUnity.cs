using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    internal static class BlazorApi
    {
        private static List<byte[]> messageBuffer = new List<byte[]>();

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
            SendMessageFromUnity(Encoding.Unicode.GetBytes("UNITY_INITIALIZED"));
        }

        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _InstantiateByteArray(int size, int id)
        {
            Debug.Log($"_InstantiateByteArray({size},{id})");
            var bytes = new byte[size];
            
            _ReadBytesBuffer(id, bytes);
            Debug.Log($"_ReadBytesBuffer({id},bytes)");
            
            Debug.Log($"Received message ({string.Join(", ",bytes)})");
            if (OnHandleReceivedMessages == null)
            {
                messageBuffer.Add(bytes);
            }
            else
            {
                OnHandleReceivedMessages?.Invoke(bytes);
            }
        }

        
        internal static Action<byte[]> OnHandleReceivedMessages
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
        
        internal static void SendMessageFromUnity(byte[] bytes)
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            throw new NotImplementedException();
#endif
            _SendMessageFromUnity(bytes,bytes.Length);
        }
    }
}