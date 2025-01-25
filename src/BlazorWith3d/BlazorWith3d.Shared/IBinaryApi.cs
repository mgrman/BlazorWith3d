using System;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApi 
    {
        Func<ArraySegment<byte>,ValueTask>? MainMessageHandler { get; set; }
        ValueTask SendMessage(IBufferWriterWithArraySegment<byte> bytes);
        Func<ArraySegment<byte>,ValueTask<IBufferWriterWithArraySegment<byte>>>? MainMessageWithResponseHandler { get; set; }
        ValueTask<ArraySegment<byte>> SendMessageWithResponse(IBufferWriterWithArraySegment<byte> bytes);
    }
}

