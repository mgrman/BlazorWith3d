using System;
using System.Buffers;
using System.Reflection;

using BlazorWith3d.Shared;

using MemoryPack;

namespace BlazorWith3d.ExampleApp.Client.Shared
{
    public class MemoryPackBinaryApiSerializer:IBinaryApiSerializer
    {
        public void SerializeObject<T>(T obj, IBufferWriter<byte> bufferWriter)
        {
            
            // manual working with this shit, as memorypack optimization for unmanaged types does not work with Typescript, to disable it
            var writerState = MemoryPackWriterOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);
            try
            {
                var writer = new MemoryPackWriter<IBufferWriter<byte>>(ref bufferWriter, writerState);
                MemoryPackSerializer.Serialize(ref writer, obj);
            }
            finally
            {
                writerState.Reset();
            }


        }

        public T? DeserializeObject<T>(ReadOnlySpan<byte> bytes, out int readBytes)
        {
            T? item=default;

            var readerState = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

            var reader = new MemoryPackReader(bytes, readerState);
            try
            {
                reader.ReadValue(ref item);
                readBytes = reader.Consumed;

                return item;
            }
            finally
            {
                reader.Dispose();
                readerState.Reset();
            }
        }
    }
}