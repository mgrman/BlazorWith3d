using System;
using System.Buffers;
using System.Text;

using BlazorWith3d.Shared;

using UnityEngine;

namespace BlazorWith3d.Unity
{
    // left here as illustration of this possibility. 
    // Uses JsonUtility, but it has limitations e.g. no property support, no nullable struct support (can be hacked around with extra field), root object must be class (but can be hacked around, see below), ....
    public class UnityJsonBinaryApiSerializer : IBinaryApiSerializer
    {
        public void SerializeObject<T>(T obj, IBufferWriter<byte> bufferWriter)
        {
            var json= JsonUtility.ToJson(obj, false);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            var headerBytes= BitConverter.GetBytes(bytes.Length);

            if (headerBytes.Length != 4)
            {
                throw new InvalidOperationException();
            }
            
            bufferWriter.Write(headerBytes);
            bufferWriter.Write(bytes);
            
        }

        public T? DeserializeObject<T>(ArraySegment<byte>  bytes, out int readBytes)
        {
            var headerBytes = BitConverter.ToInt32(bytes.Slice(0, 4));
            
            var json= Encoding.UTF8.GetString(bytes.Slice(4, headerBytes));

            readBytes = headerBytes + 4;

            // limitation, as values cannot be serialized or deserialized via JsonUtility, or write custom deserialization
            if (typeof(T) == typeof(float) || typeof(T) == typeof(int))
            {
                json = $"{{ \"value\":{json} }}";

                var valueWrapper = new ValueWrapper<T>();

                JsonUtility.FromJsonOverwrite(json, valueWrapper);
                    return valueWrapper.value;
            }

            return JsonUtility.FromJson<T>(json);
        }
        
        [Serializable]
        private class ValueWrapper<T>
        {
            public T value = default!;
        }
    }
}