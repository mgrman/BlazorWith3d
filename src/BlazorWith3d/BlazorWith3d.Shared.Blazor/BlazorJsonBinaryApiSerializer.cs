using System.Buffers;
using System.Text;
using System.Text.Json;

namespace BlazorWith3d.Shared.Blazor;

// left here as illustration of this possibility. 
// Uses System.Text.Json adjusted to support fields for Unity JsonUtility, but can be adjusted to other json libraries.
public class BlazorJsonBinaryApiSerializer : IBinaryApiSerializer
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions() { IncludeFields = true };

    public void SerializeObject<T>(T obj, IBufferWriter<byte> bufferWriter)
    {
        if (bufferWriter is IBufferWriterWithArraySegment<byte> writerWithArraySegment)
        {
            var position = writerWithArraySegment.WrittenArray.Count;
            bufferWriter.Advance(sizeof(int));

            int length = 0;
            using (var writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Indented = false }))
            {
                JsonSerializer.Serialize(writer, obj, _options);
                writer.Flush();
                length = (int)writer.BytesCommitted;
            }

            var headerBytes = BitConverter.GetBytes(length);
            
            
            headerBytes.CopyTo(writerWithArraySegment.WrittenArray.Slice(position).AsSpan());
        }
        else
        {

            var json = JsonSerializer.Serialize(obj, _options);
            var bytes = Encoding.UTF8.GetBytes(json);

            var headerBytes = BitConverter.GetBytes(bytes.Length);

            if (headerBytes.Length != 4)
            {
                throw new InvalidOperationException();
            }

            bufferWriter.Write(headerBytes);
            bufferWriter.Write(bytes);
        }
    }

    public T DeserializeObject<T>(ArraySegment<byte> bytes, out int readBytes)
    {
        var headerBytes = BitConverter.ToInt32(bytes.Slice(0, sizeof(int)));

        var jsonBytes = bytes.Slice(sizeof(int), headerBytes);

        readBytes = headerBytes + sizeof(int);

        var result= JsonSerializer.Deserialize<T>(jsonBytes, _options)!;
        return result;
    }
}