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

    public T DeserializeObject<T>(ArraySegment<byte> bytes, out int readBytes)
    {
        var headerBytes = BitConverter.ToInt32(bytes.Slice(0, 4));

        var json = Encoding.UTF8.GetString(bytes.Slice(4, headerBytes));

        readBytes = headerBytes + 4;

        var result= JsonSerializer.Deserialize<T>(json, _options)!;
        return result;
    }
}