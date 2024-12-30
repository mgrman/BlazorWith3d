using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.Unity.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer, IDisposable
{
    private IBlocksOnGrid3DApp? unityAppApi;
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
            unityAppApi = new BlocksOnGrid3DApp_BinaryApiWithResponse(this);
        }
        else
        {
            unityAppApi = new BlocksOnGrid3DApp_BinaryApi(this);
        }

        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            Logger.LogError($"Error deserializing message {bytes}", exception);
        };

        _rendererAssignment = await ParentApp.InitializeRenderer(unityAppApi, async () =>
        {
            await InitializeUnityApp();
        });

        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        (unityAppApi as IDisposable)?.Dispose();
        _rendererAssignment?.Dispose();
    }
}