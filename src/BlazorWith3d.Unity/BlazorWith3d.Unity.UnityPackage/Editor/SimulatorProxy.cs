using System;
using System.Drawing.Printing;
using System.Threading.Tasks;
using BlazorWith3d.Unity.Shared;

namespace BlazorWith3d.Unity
{
    public class SimulatorProxy 
    {
        public SimulatorProxy()
        {
            var blazorApi = new BinaryApiProxy();
            BlazorApi = blazorApi;
            var unityApi = new BinaryApiProxy();
            UnityApi = unityApi;

            blazorApi.On_SendMessage += unityApi.Invoke_OnMessage;
            unityApi.On_SendMessage += blazorApi.Invoke_OnMessage;
        }
        
        public IBinaryApi BlazorApi { get; }
        
        public IBinaryApi UnityApi { get; }
        
        private class BinaryApiProxy:IBinaryApi
        {
            public event Action<byte[]> OnMessage;
            public ValueTask SendMessage(byte[] bytes)
            {
                On_SendMessage?.Invoke(bytes);
                return new ValueTask();
            }

            public event Action<byte[]> On_SendMessage;

            public void Invoke_OnMessage(byte[] bytes)
            {
                OnMessage?.Invoke(bytes);
            }
        }
    }
}