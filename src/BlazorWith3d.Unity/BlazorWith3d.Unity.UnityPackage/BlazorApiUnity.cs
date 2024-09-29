using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    internal static class BlazorApi
    {
        private static int _responseIdCounter = 0;

        private static IDictionary<int, AwaitableCompletionSource<string>> _responseAwaitables =
            new Dictionary<int, AwaitableCompletionSource<string>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static async void OnBeforeSplashScreen()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;

            Debug.Log($"On before BlazorApiUnity.InitializeApi");
            InitializeApi(OnMessageReceived);

            Debug.Log($"On after BlazorApiUnity.InitializeApi");
            var response = await SendMessageFromUnity("UNITY_INITIALIZED");
            Debug.Log($"UNITY_INITIALIZED:{response}");
#endif
        }

        [MonoPInvokeCallback(typeof(Func<string, string>))]
        private static string OnMessageReceived(string msg)
        {
            return TypedMessageBlazorApi.HandleReceivedMessages(msg);
        }

        [MonoPInvokeCallback(typeof(Action<int, string>))]
        private static async void OnResponseReceived(int msgId, string response)
        {
            await Awaitable.MainThreadAsync();
            if (_responseAwaitables.TryGetValue(msgId, out var tcs))
            {
                tcs.TrySetResult(response);
                _responseAwaitables.Remove(msgId);
            }
        }

        [DllImport("__Internal")]
        private static extern void SendMessageFromUnity(int msgId, string message,
            Action<int, string> onResponseReceived);

        [DllImport("__Internal")]
        private static extern string InitializeApi(Func<string, string> onMessageReceived);

        internal static Awaitable<string> SendMessageFromUnity(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int msgId;
            unchecked
            {

                msgId = _responseIdCounter++;
            }

            var awaitableTcs = new AwaitableCompletionSource<string>();
            _responseAwaitables[msgId] = awaitableTcs;
            SendMessageFromUnity(msgId, message, OnResponseReceived);
            return awaitableTcs.Awaitable;
#else
            throw new NotImplementedException();
#endif
        }

        //public static Func<string, string> ProcessReceivedMessage { get; set; } not overridable for now, unnecessary complexity to support
    }
}