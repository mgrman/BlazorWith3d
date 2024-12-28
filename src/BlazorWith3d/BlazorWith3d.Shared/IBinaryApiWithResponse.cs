using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApiWithResponse
    {
        Func<byte[],ValueTask>? MainMessageHandler { get; set; }
        Func<byte[],ValueTask<byte[]>>? MainMessageWithResponseHandler { get; set; }
        ValueTask SendMessage(byte[] bytes);
        ValueTask<byte[]> SendMessageWithResponse(byte[] bytes);
    }
}

