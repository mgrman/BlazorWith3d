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

        private static readonly IDictionary<int, AwaitableCompletionSource<string>> _responseAwaitables =
            new Dictionary<int, AwaitableCompletionSource<string>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static async void OnBeforeSplashScreen()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;

            Debug.Log($"On before BlazorApiUnity.InitializeApi");
            _InitializeApi(_OnMessageReceivedWithResponse, _OnMessageReceived);

            Debug.Log($"On after BlazorApiUnity.InitializeApi");
            var response = await SendMessageWithResponseFromUnityAsync("UNITY_INITIALIZED");
            Debug.Log($"UNITY_INITIALIZED:{response}");
#endif
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void _OnMessageReceived(string msg)
        {
            TypedMessageBlazorApi.HandleReceivedMessages(msg);
        }

        [MonoPInvokeCallback(typeof(Func<string, string>))]
        private static string _OnMessageReceivedWithResponse(string msg)
        {
            return TypedMessageBlazorApi.HandleReceivedMessagesWithResponse(msg);
        }

        [MonoPInvokeCallback(typeof(Action<int, string>))]
        private static async void OnResponseReceived(int msgId, string response)
        {
            Debug.Log($"Received response for msg {msgId} as {response}");
            await Awaitable.MainThreadAsync();
            if (_responseAwaitables.TryGetValue(msgId, out var tcs))
            {
                tcs.TrySetResult(response);
                _responseAwaitables.Remove(msgId);
            }
        }

        [DllImport("__Internal")]
        private static extern void _SendMessageWithResponseFromUnity(int msgId, string message,
            Action<int, string> onResponseReceived);

        [DllImport("__Internal")]
        private static extern void _SendMessageFromUnity(string message);

        [DllImport("__Internal")]
        private static extern string _InitializeApi(Func<string, string> onMessageReceivedWithResponse,Action<string> onMessageReceived);

        internal static Awaitable<string> SendMessageWithResponseFromUnityAsync(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int msgId;
            unchecked
            {

                msgId = _responseIdCounter++;
            }

            Debug.Log($"Sending msg {msgId} as {message}");

            var awaitableTcs = new AwaitableCompletionSource<string>();
            _responseAwaitables[msgId] = awaitableTcs;
            _SendMessageWithResponseFromUnity(msgId, message, OnResponseReceived);
            return awaitableTcs.Awaitable;
#else
            throw new NotImplementedException();
#endif
        }
        internal static void SendMessageFromUnity(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _SendMessageFromUnity(message);
#else
            throw new NotImplementedException();
#endif
        }

        //public static Func<string, string> ProcessReceivedMessage { get; set; } not overridable for now, unnecessary complexity to support
    }
}