using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSRenderer: BaseJsRenderer, IBlocksOnGrid3DBlazorRenderer, IAsyncDisposable
{
    private BlocksOnGrid3DRendererOverBinaryApi? _appApi;
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
        _appApi = new BlocksOnGrid3DRendererOverBinaryApi(_binaryApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory(), ParentApp);
        _appApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        
        await ParentApp.AddRenderer(this);
        
        await base.OnAfterRenderAsync(firstRender);
    }

    IBlocksOnGrid3DRenderer IBlocksOnGrid3DBlazorRenderer.RendererApi => _appApi;
    ElementReference IBlocksOnGrid3DBlazorRenderer.RendererContainer => _containerElementReference;


    public async ValueTask DisposeAsync()
    {
        if (_appApi != null)
        {
            await ParentApp.RemoveRenderer(this);
        }

        _appApi?.Dispose();
        await _binaryApi.TryDisposeAsync();
    }
}