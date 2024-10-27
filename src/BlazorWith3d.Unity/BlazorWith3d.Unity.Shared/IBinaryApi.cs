using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Unity.Shared
{
    public interface IBinaryApi
    {
        event Action<byte[]>? OnMessage;
        ValueTask SendMessage(byte[] bytes);
    }
}