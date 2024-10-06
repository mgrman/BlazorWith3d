using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity
{
    public interface IUnityApi
    {
        Action<byte[]> OnMessageFromUnity { get; set; }
        void SendMessageToUnity(byte[] bytes);
    }
}