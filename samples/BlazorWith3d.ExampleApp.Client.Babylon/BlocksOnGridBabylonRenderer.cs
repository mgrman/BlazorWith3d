using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.Babylon;

public class BlocksOnGridBabylonRenderer:BaseJsRenderer, IAsyncDisposable
{
    private BlocksOnGrid3DRendererOverBinaryApi? _appApi;
    private JsBinaryApiWithResponseRenderer? _binaryApi;

    [CascadingParameter] 
    public required IBlocksOnGrid3DControllerApp ParentApp { get; set; }
    
    
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
        
        await _binaryApi.InitializeJsApp(JsAppPath, _containerElementReference);
        
        _appApi = new BlocksOnGrid3DRendererOverBinaryApi(_binaryApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
        _appApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        var eventHandler=await ParentApp.AddRenderer(_appApi);
        await _appApi.SetEventHandler(eventHandler);
        
        await _binaryApi.OnConnectedToController();
        
    }

    public async ValueTask DisposeAsync()
    {
        _appApi?.Dispose();
        await _binaryApi.TryDisposeAsync();
    }
}