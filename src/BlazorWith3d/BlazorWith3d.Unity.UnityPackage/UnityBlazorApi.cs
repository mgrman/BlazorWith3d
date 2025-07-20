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
        private static Dictionary<int, AwaitableCompletionSource<byte[]>> s_responses=new ();
        
        
        private static Func<ArraySegment<byte>, ValueTask>? s_mainMessageHandler;

        public static Func<ArraySegment<byte>, ValueTask>? MainMessageHandler
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
            }
        }
        private static Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? s_mainMessageWithResponseHandler;

        public static Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler
        {
            get => s_mainMessageWithResponseHandler;
            set
            {
                if (s_mainMessageWithResponseHandler != null)
                {
                    throw new InvalidOperationException("Cannot set MainMessageHandler on a Unity Blazor again after it was set!");
                }
                if (value == null)
                {
                    throw new InvalidOperationException("Cannot set MainMessageHandler to null!");
                }
                s_mainMessageWithResponseHandler = value;
            }
        }

        public static void InitializeWebGLInterop()
        {
            if (MainMessageHandler == null)
            {
                throw new InvalidOperationException("MainMessageHandler must be set before InitializeWebGLInterop is called!");
            }
            
            Debug.Log("On before BlazorApiUnity.InitializeApi");
            _InitializeApi(_ReadMessage,MainMessageWithResponseHandler==null?null:_ReadMessageWithResponse,MainMessageWithResponseHandler==null?null: _ReadResponse);
        }

        Func<ArraySegment<byte>, ValueTask>? IBinaryApi.MainMessageHandler { 
            get => MainMessageHandler;
            set => MainMessageHandler = value;
        }

        Func<ArraySegment<byte>, ValueTask<IBufferWriterWithArraySegment<byte>>>? IBinaryApi.MainMessageWithResponseHandler { 
            get => MainMessageWithResponseHandler;
            set => MainMessageWithResponseHandler = value;
        }

        private UnityBlazorApi()
        {
            
        }

        public static UnityBlazorApi Singleton { get; } = new UnityBlazorApi();

        public async ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes)
        {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            if (Application.isEditor|| Application.platform != RuntimePlatform.WebGLPlayer)
            {
                throw new NotImplementedException();
            }
#endif
            
            var msg = bytes.WrittenArray;
            _SendMessageFromUnity(msg.Array,msg.Offset, msg.Count);
            bytes.Dispose();
        }
        
        public async ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> bytes)
        {
            int id = _GetNextRequestId();

            var tcs = new AwaitableCompletionSource<byte[]>();
            s_responses[id] = tcs;
            
            var msg = bytes.WrittenArray;
            _SendMessageWithResponseFromUnity(id, msg.Array, msg.Offset,msg.Count);
            bytes.Dispose();
            var result=await tcs.Awaitable;

            s_responses.Remove(id);
            return result;
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
        }

        // optimization, so the array is created on Unity side, and exposed to emscripten JS interop code to fill in.
        // see https://stackoverflow.com/a/71472662
        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _ReadMessage(int size, int id)
        {
            var bytes = new byte[size];

            _ReadBytesBuffer(id, bytes);

            if (MainMessageHandler == null)
            {
                Debug.LogError("Singleton.MainMessageHandler is not set!");
                throw new InvalidOperationException();
            }
            MainMessageHandler.Invoke(bytes);
        }

        // optimization, so the array is created on Unity side, and exposed to emscripten JS interop code to fill in.
        // see https://stackoverflow.com/a/71472662
        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static async void _ReadMessageWithResponse(int size, int id)
        {
            var bytes = new byte[size];

            _ReadBytesBuffer(id, bytes);

            if (MainMessageWithResponseHandler == null)
            {
                Debug.LogError("Singleton.MainMessageHandler is not set!");
                throw new InvalidOperationException();
            }

            var response = await MainMessageWithResponseHandler.Invoke(bytes);

            if (response.WrittenArray.Array == null)
            {
                throw new InvalidOperationException("response.WrittenArray.Array cannot be null!");
            }
            
            _SendResponseFromUnity(id, response.WrittenArray.Array,response.WrittenArray.Offset, response.WrittenArray.Count);
            response.Dispose();
        }

        // optimization, so the array is created on Unity side, and exposed to emscripten JS interop code to fill in.
        // see https://stackoverflow.com/a/71472662
        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void _ReadResponse(int size, int id)
        {
            var bytes = new byte[size];

            _ReadBytesBuffer(id, bytes);

            s_responses[id].SetResult(bytes);
        }

        [DllImport("__Internal")]
        private static extern int _GetNextRequestId();

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(byte[] message, int offset, int size);

        [DllImport("__Internal")]
        private static extern void _SendMessageWithResponseFromUnity(int id, byte[] message, int offset, int size);

        [DllImport("__Internal")]
        private static extern void _SendResponseFromUnity(int id,byte[] message, int offset, int size);

        [DllImport("__Internal")]
        private static extern void _ReadBytesBuffer(int id, byte[] array);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Action<int, int> readMessageCallback,Action<int, int>? readMessageWithResponseCallback,Action<int, int>? readResponseCallback);
    }
}