using System;
using System.Buffers;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApiSerializer
    {
        void SerializeObject<T>(T obj, IBufferWriter<byte> writer);

        T DeserializeObject<T>(ArraySegment<byte> bytes, out int readBytes);
    }
}