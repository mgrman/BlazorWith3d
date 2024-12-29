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
        private static Func<byte[], ValueTask>? s_mainMessageHandler;

        public static Func<byte[], ValueTask>? MainMessageHandler
        {
            get => s_mainMessageHandler;
            set
            {
                if (s_mainMessageHandler != null)
                {
                    throw new InvalidOperationException("Cannot set MainMessageHandler on a Unity Blazor again after it was set!");
                }
                if (value == null)
                {
                    throw new InvalidOperationException("Cannot set MainMessageHandler to null!");
                }
                s_mainMessageHandler = value;
                
                Debug.Log("On before BlazorApiUnity.InitializeApi");
                _InitializeApi(_InstantiateByteArray);
            }
        }

        Func<byte[], ValueTask>? IBinaryApi.MainMessageHandler { 
            get => MainMessageHandler;
            set => MainMessageHandler = value;
        }


        private UnityBlazorApi()
        {
            
        }

        public static UnityBlazorApi Singleton { get; } = new UnityBlazorApi();

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
        }

        // optimization, so the array is created on Unity side, and exposed to emscripten JS interop code to fill in.
        // see https://stackoverflow.com/a/71472662
        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _InstantiateByteArray(int size, int id)
        {
            var bytes = new byte[size];

            _ReadBytesBuffer(id, bytes);

            if (MainMessageHandler == null)
            {
                Debug.LogError("Singleton.MainMessageHandler is not set!");
                throw new InvalidOperationException();
            }
            MainMessageHandler?.Invoke(bytes);
        }

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(byte[] message, int size);

        [DllImport("__Internal")]
        private static extern void _ReadBytesBuffer(int id, byte[] array);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Action<int, int> instantiateByteArrayCallback);
    }
}