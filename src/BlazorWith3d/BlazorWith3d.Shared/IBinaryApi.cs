using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace BlazorWith3d.Shared
{
    public interface IBinaryApi
    {
        Func<byte[],ValueTask>? MainMessageHandler { get; set; }
        ValueTask SendMessage(byte[] bytes);
    }
}

