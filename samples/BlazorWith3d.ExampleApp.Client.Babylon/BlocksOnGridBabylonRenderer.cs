using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.Babylon;

public class BlocksOnGridBabylonRenderer:BaseJsRenderer, IAsyncDisposable
{
    private BlocksOnGrid3DRenderer_BinaryApiWithResponse? _unityAppApi;
    private IDisposable? _rendererAssignment;
    private JsBinaryApiWithResponseRenderer? _binaryApi;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; }

    [Inject] 
    protected ILogger<BlocksOnGridBabylonRenderer> _logger { get; set; }

    private string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Babylon/clientassets/blazorwith3d-exampleapp-client-babylon-bundle.js"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        await base.OnAfterRenderAsync(firstRender);

        _binaryApi=new JsBinaryApiWithResponseRenderer(_jsRuntime,_logger);
        
        _unityAppApi = new BlocksOnGrid3DRenderer_BinaryApiWithResponse(_binaryApi, new MemoryPackBinaryApiSerializer(),
            new PoolingArrayBufferWriterFactory());
        _unityAppApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };

        _rendererAssignment = await ParentApp.InitializeRenderer(_unityAppApi, async () =>
        {
           await _binaryApi.InitializeJsApp(JsAppPath, _containerElementReference);
        });
    }

    public async ValueTask DisposeAsync()
    {
        _unityAppApi?.Dispose();
        _rendererAssignment?.Dispose();
        await _binaryApi.TryDisposeAsync();
    }
}