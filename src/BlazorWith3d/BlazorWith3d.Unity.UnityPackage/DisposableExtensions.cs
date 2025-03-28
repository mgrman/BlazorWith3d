using System;

using BlazorWith3d.Shared;

using UnityEngine;

namespace BlazorWith3d.Unity
{
    public static class DisposableExtensions
    {
        public static IDisposable DestroyAsDisposable(this GameObject obj)
        {
            return new DisposableAction(() =>
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    GameObject.Destroy(obj);
                }
                else
                {
                    GameObject.DestroyImmediate(obj);
                }
#else
                    GameObject.Destroy(obj);
#endif
                
            });
        }
        public static void TrackDestroy(this DisposableContainer container, GameObject obj)
        {
            container.TrackDisposable(obj.DestroyAsDisposable());
        }
    }
}