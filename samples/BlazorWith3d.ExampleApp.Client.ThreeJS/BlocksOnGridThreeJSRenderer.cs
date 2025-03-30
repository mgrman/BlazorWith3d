using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSRenderer: BaseJsRenderer, IAsyncDisposable
{
    private BlocksOnGrid3DRendererOverBinaryApi? _unityAppApi;
    private JsBinaryApiWithResponseRenderer _binaryApi;

    [CascadingParameter] 
    public required IBlocksOnGrid3DBlazorController ParentApp { get; set; }
    
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; }

    [Inject] 
    protected ILogger<BlocksOnGridThreeJSRenderer> _logger { get; set; }
    
    [Parameter]
    public bool CopyArrays { get; set; } = true;
    
    private string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS/clientassets/blazorwith3d-exampleapp-client-threejs-bundle.js"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        if (CopyArrays)
        {
            _binaryApi = new JsBinaryApiWithResponseRenderer(_jsRuntime, _logger);
        }
        else
        {
            _binaryApi = new JsBinaryApiWithResponseRendererWithoutCopy(_jsRuntime, _logger);
        }

        await _binaryApi.InitializeJsApp(JsAppPath, _containerElementReference);
        _unityAppApi = new BlocksOnGrid3DRendererOverBinaryApi(_binaryApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory(), ParentApp);
        _unityAppApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        
        await ParentApp.AddRenderer(new BlocksOnGrid3DBlazorRenderer(_unityAppApi, _containerElementReference));
        
        await base.OnAfterRenderAsync(firstRender);
    }



    public async ValueTask DisposeAsync()
    {
        if (_unityAppApi != null)
        {
            await ParentApp.RemoveRenderer(_unityAppApi);
        }

        _unityAppApi?.Dispose();
        await _binaryApi.TryDisposeAsync();
    }
}