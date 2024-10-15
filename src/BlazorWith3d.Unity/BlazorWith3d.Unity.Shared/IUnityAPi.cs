using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity.Shared
{
    public interface IUnityApi
    {
        Action<byte[]>? OnMessageFromUnity { get; set; }
        Task SendMessageToUnity(byte[] bytes);
    }
}