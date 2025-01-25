using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryMessageApi
    {
        Func<ArraySegment<byte>,ValueTask>? MainMessageHandler { get; set; }
        ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes);
    }
}