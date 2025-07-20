using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsRenderer;
using BlazorWith3d.Shared.Blazor;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.Unity.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer, IBlocksOnGrid3DBlazorRenderer, IAsyncDisposable
{
    private BlocksOnGrid3DRendererOverBinaryApi? _unityAppApi;
    private JsBinaryApiWithResponseRenderer? _binaryApi;
    
    [CascadingParameter] 
    public required IBlocksOnGrid3DBlazorController ParentApp { get; set; }
    
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; }
    
    [Inject]
    protected ILogger<BlocksOnGridUnityRenderer> _logger { get; set; }
    
    [Parameter]
    public bool UseJsonSerializer { get; set; }

    public string UnityBuildFilesRootPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Unity"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }
        
        _binaryApi=new JsBinaryApiWithResponseRenderer(_jsRuntime,_logger);
        
        await _binaryApi.InitializeJsApp(this._unityInitializationJsPath, _containerElementReference,"showUnity", GetExtraArg(UnityBuildFilesRootPath, UseJsonSerializer?"json":"memoryPack"));
        var unityAppApi = new BlocksOnGrid3DRendererOverBinaryApi(_binaryApi,UseJsonSerializer?new BlazorJsonBinaryApiSerializer(): new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory(),ParentApp);
        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        _unityAppApi=unityAppApi;
        
        await ParentApp.AddRenderer(this);
        
        
        
        await base.OnAfterRenderAsync(firstRender);
    }
    IBlocksOnGrid3DRenderer IBlocksOnGrid3DBlazorRenderer.RendererApi => _unityAppApi;
    ElementReference IBlocksOnGrid3DBlazorRenderer.RendererContainer => _containerElementReference;

    public async ValueTask DisposeAsync()
    {
        if (_unityAppApi != null)
        {
            await ParentApp.RemoveRenderer(this);
        }
        _unityAppApi?.Dispose();
        _binaryApi?.TryDisposeAsync();
    }

    public string Label => $"{nameof(BlocksOnGridUnityRenderer)}({(UseJsonSerializer ? "JSON" : "MemoryPack")})";
}