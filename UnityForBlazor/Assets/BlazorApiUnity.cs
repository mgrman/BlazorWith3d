using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

public static class BlazorApiUnity
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void OnBeforeSplashScreen()
    {

        Debug.LogWarning($"On before InitializeApi");
        InitializeApi(OnMessageReceived);
        
        Debug.LogWarning($"On after InitializeApi");
        
        SendMessageFromUnity(0,"UNITY_INITIALIZED",OnResponseReceived);
    }

    [MonoPInvokeCallback(typeof(Func<string,string>))]
    private static string OnMessageReceived(string arg)
    {
        return $"ACK for {arg}";
    }

    [MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnResponseReceived(int msgId, string response)
    {
        Debug.LogWarning($"UNITY_INITIALIZED:"+response);
    }
    
    [DllImport("__Internal")]
    private static extern string SendMessageFromUnity(int msgId,string message,Action<int,string> onResponseReceived);
    
    [DllImport("__Internal")]
    private static extern string InitializeApi(Func<string,string> onMessageReceived);

}
