using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApiWithResponse:IBinaryApi
    {
        Func<byte[],ValueTask<byte[]>>? MainMessageWithResponseHandler { get; set; }
        ValueTask<byte[]> SendMessageWithResponse(byte[] bytes);
    }
}

