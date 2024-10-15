using System;

namespace BlazorWith3d.Unity.Shared
{
    public interface IBlazorApi
    {
        Action<byte[]> OnMessageFromBlazor { get; set; }
        void SendMessageToBlazor(byte[] bytes);
    }
}