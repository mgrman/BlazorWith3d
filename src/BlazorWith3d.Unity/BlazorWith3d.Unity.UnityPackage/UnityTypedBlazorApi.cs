using System;
using System.Collections.Generic;
using System.Text;
using BlazorWith3d.Unity.Shared;
using MemoryPack;
using UnityEngine;

namespace BlazorWith3d.Unity
{
    // not in Shared package, as it uses too many Unity specific APIs, mainly Awaitables
    public class UnityTypedBlazorApi : TypedBlazorApi
    {
        private readonly IBlazorApi _blazorApi;

        private readonly IDictionary<Type, Action<object>> _handlers = new Dictionary<Type, Action<object>>();

        public UnityTypedBlazorApi(IBlazorApi blazorApi)
            : base(blazorApi)
        {
        }

        protected override byte[] SerializeObject<T>(T obj)
        {
            return MemoryPackSerializer.Serialize<IMessageToBlazor>(obj);
        }

        protected override object DeserializeObject(byte[] obj)
        {
            return MemoryPackSerializer.Deserialize<IMessageToUnity>(obj);
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
    }
}