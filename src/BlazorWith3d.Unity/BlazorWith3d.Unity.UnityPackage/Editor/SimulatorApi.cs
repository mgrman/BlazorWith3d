using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity
{
    public class SimulatorApi: IBlazorApi, IUnityApi
    {
        public Action<byte[]> OnMessageFromBlazor { get; set; }
        public void SendMessageToBlazor(byte[] bytes)
        {
            OnMessageFromUnity?.Invoke(bytes);
        }

        public Action<byte[]> OnMessageFromUnity { get; set; }
        public Task SendMessageToUnity(byte[] bytes)
        {
            OnMessageFromBlazor?.Invoke(bytes);
            return Task.CompletedTask;
        }
    }
}