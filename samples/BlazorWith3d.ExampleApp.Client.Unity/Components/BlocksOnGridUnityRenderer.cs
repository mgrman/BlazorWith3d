using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;
using BlazorWith3d.Shared;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.Unity.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer, IDisposable
{
    private BlocksOnGrid3DRendererOverBinaryApi? _unityAppApi;
    private JsBinaryApiWithResponseRenderer? _binaryApi;
    
    [CascadingParameter] 
    public required IBlocksOnGrid3DControllerApp ParentApp { get; set; }
    
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; }
    
    [Inject]
    protected ILogger<BlocksOnGridUnityRenderer> _logger { get; set; }

    public string UnityBuildFilesRootPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Unity"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }
        
        _binaryApi=new JsBinaryApiWithResponseRenderer(_jsRuntime,_logger);
        
        await _binaryApi.InitializeJsApp(this._unityInitializationJsPath, _containerElementReference,"showUnity", GetExtraArg(UnityBuildFilesRootPath, false));
        var unityAppApi = new BlocksOnGrid3DRendererOverBinaryApi(_binaryApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        _unityAppApi=unityAppApi;
        
        
        var eventHandler=await ParentApp.AddRenderer(_unityAppApi);
        await _unityAppApi.SetEventHandler(eventHandler);
        
        await _binaryApi.OnConnectedToController();
        
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        _unityAppApi?.Dispose();
        _binaryApi?.TryDisposeAsync();
    }
}