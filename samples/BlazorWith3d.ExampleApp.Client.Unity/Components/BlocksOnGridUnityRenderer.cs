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
    private BlocksOnGrid3DRenderer_BinaryApiWithResponse? _unityAppApi;
    private IDisposable? _rendererAssignment;
    private IInitializableJsBinaryApi? _binaryApi;
    
    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    [Parameter]
    public bool IsWithResponse { get; set; }
    
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

        IBinaryApiWithResponse binaryApi;
        IInitializableJsBinaryApi initializableBinaryApi;
        if (!IsWithResponse)
        {
            var jsBinaryApi=new JsBinaryApiRenderer(_jsRuntime,_logger);
            binaryApi=new BinaryApiWithResponseOverBinaryApi(jsBinaryApi);
            initializableBinaryApi=jsBinaryApi;
            _binaryApi=jsBinaryApi;
        }
        else
        {
            var jsBinaryApi=new JsBinaryApiWithResponseRenderer(_jsRuntime,_logger);
            binaryApi=jsBinaryApi;
            initializableBinaryApi=jsBinaryApi;
            _binaryApi=jsBinaryApi;
        }

        var unityAppApi = new BlocksOnGrid3DRenderer_BinaryApiWithResponse(binaryApi, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            _logger.LogError($"Error deserializing message {bytes}", exception);
        };
        _unityAppApi=unityAppApi;


        _rendererAssignment = await ParentApp.InitializeRenderer(_unityAppApi, async () =>
        {
            await initializableBinaryApi.InitializeJsApp(this._unityInitializationJsPath, _containerElementReference,"showUnity", GetExtraArg(UnityBuildFilesRootPath, IsWithResponse));
        });

        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        _unityAppApi?.Dispose();
        _rendererAssignment?.Dispose();
    }
}