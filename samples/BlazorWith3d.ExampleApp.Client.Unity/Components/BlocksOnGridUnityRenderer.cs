using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.Unity.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer, IDisposable
{
    private BlocksOnGrid3DApp_BinaryApi? unityAppApi;

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

        unityAppApi = new BlocksOnGrid3DApp_BinaryApi(this);
        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            Logger.LogError($"Error deserializing message {bytes}", exception);
        };
        
        ParentApp.InitializeRenderer(unityAppApi);
        
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        if (unityAppApi != null)
        {
            ParentApp.InitializeRenderer(null);
            unityAppApi.Dispose();
        }
    }
}