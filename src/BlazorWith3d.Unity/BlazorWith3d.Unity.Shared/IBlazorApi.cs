using System;

namespace BlazorWith3d.Unity
{
    public interface IBlazorApi
    {
        Action<byte[]> OnHandleReceivedMessages { get; set; }
        void SendMessageFromUnity(byte[] bytes);
    }
}