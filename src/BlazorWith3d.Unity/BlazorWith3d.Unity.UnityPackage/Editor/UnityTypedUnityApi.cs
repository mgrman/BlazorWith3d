using System;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    // not in Shared package, as it uses too many Unity specific APIs, mainly Awaitables
    public class UnityTypedUnityApi:TypedUnityApi
    {
        public UnityTypedUnityApi(IUnityApi unityApi) : base(unityApi)
        {
        }

        protected override void LogError(Exception exception, string msg)
        {
            Debug.LogException(exception);
            Debug.LogError(msg);
        }

        protected override void LogError(string msg)
        {
            Debug.LogError(msg);
        }

        protected override void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }

        protected override void Log(string msg)
        {
            Debug.Log(msg);
        }

        protected override string SerializeObject<T>(T obj)
        {
            return JsonUtility.ToJson(obj);
        }

        protected override T DeserializeObject<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}