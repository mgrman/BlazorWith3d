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
        private static List<string> messageBuffer = new List<string>();

        private static Action<string> _onHandleReceivedMessages;
        
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
            SendMessageFromUnity("UNITY_INITIALIZED");
        }

        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _InstantiateByteArray(int size, int id)
        {
            Debug.Log($"_InstantiateByteArray({size},{id})");
            var bytes = new byte[size];
            
            _ReadBytesBuffer(id, bytes);
            Debug.Log($"_ReadBytesBuffer({id},bytes)");
            
            var message = Encoding.Unicode.GetString(bytes);
            
            
            Debug.Log($"Received message ({string.Join(", ",bytes)})");
            Debug.Log($"Received message ({message})");
            if (OnHandleReceivedMessages == null)
            {
                messageBuffer.Add(message);
            }
            else
            {
                OnHandleReceivedMessages?.Invoke(message);
            }
        }

        
        internal static Action<string> OnHandleReceivedMessages
        {
            get => _onHandleReceivedMessages;
            set
            {
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
        
        internal static void SendMessageFromUnity(string message)
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            throw new NotImplementedException();
#endif
            var btyes = Encoding.Unicode.GetBytes(message);
            
            _SendMessageFromUnity(btyes,btyes.Length);
        }

        //public static Func<string, string> ProcessReceivedMessage { get; set; } not overridable for now, unnecessary complexity to support
    }
}