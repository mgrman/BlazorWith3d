using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Shared;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.Unity.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer, IDisposable
{
    private IBlocksOnGrid3DRenderer? _unityAppApi;
    private IDisposable? _rendererAssignment;
    
    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    [Inject]
    public required ILogger<BlocksOnGridUnityRenderer> Logger { get; set; }

    public override string UnityBuildFilesRootPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Unity"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        if (IsWithResponse)
        {
            var unityAppApi = new BlocksOnGrid3DRenderer_BinaryApiWithResponse(this, new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
            unityAppApi.OnMessageError += (bytes, exception) =>
            {
                Logger.LogError($"Error deserializing message {bytes}", exception);
            };
            _unityAppApi=unityAppApi;
        }
        else
        {
            var unityAppApi = new BlocksOnGrid3DRenderer_BinaryApiWithResponse(new BinaryApiWithResponseOverBinaryApi(this), new MemoryPackBinaryApiSerializer(), new PoolingArrayBufferWriterFactory());
            unityAppApi.OnMessageError += (bytes, exception) =>
            {
                Logger.LogError($"Error deserializing message {bytes}", exception);
            };
            _unityAppApi=unityAppApi;
        }


        _rendererAssignment = await ParentApp.InitializeRenderer(_unityAppApi, async () =>
        {
            await InitializeUnityApp();
        });

        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        (_unityAppApi as IDisposable)?.Dispose();
        _rendererAssignment?.Dispose();
    }
}