using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorWith3d.JsApp;

public class JsBinaryApiWithResponseRendererWithoutCopy: JsBinaryApiWithResponseRenderer
{
    public JsBinaryApiWithResponseRendererWithoutCopy(IJSRuntime jsRuntime, ILogger<BaseJsRenderer> logger) : base(jsRuntime, logger)
    {
    }

    protected override (byte[] array, int offset, int count) GetArraysForInterop(IBufferWriterWithArraySegment<byte> messageBytes)
    {
        return (messageBytes.WrittenArray.Array!, messageBytes.WrittenArray.Offset, messageBytes.WrittenArray.Count);
    }
}