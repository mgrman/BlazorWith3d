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
        
        var response=SendMessageFromUnity("UNITY_INITIALIZED");
        Debug.LogWarning($"UNITY_INITIALIZED:"+response);
    }

    [MonoPInvokeCallback(typeof(Func<string,string>))]
    private static string OnMessageReceived(string arg)
    {
        return $"ACK for {arg}";
    }
    
    [DllImport("__Internal")]
    private static extern string SendMessageFromUnity(string message);
    
    [DllImport("__Internal")]
    private static extern string InitializeApi(Func<string,string> onMessageReceived);

}
