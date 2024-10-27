using BlazorWith3d.ExampleApp.Client.Unity.Shared;
using BlazorWith3d.Unity;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.Client.Components;

public class BlocksOnGridUnityRenderer:BaseUnityRenderer
{
    [CascadingParameter] 
    public I3DAppController ParentApp { get; set; }
    
    [Inject]
    public ILogger<BlocksOnGridUnityRenderer> Logger { get; set; }
    
    public override string UnityBuildFilesRootPath => "./_content/BlazorWith3d.ExampleApp.Client.Unity";

    protected override void OnInitialized()
    {
        BlocksOnGrid3DApp.InitializeMemoryPack();
        base.OnInitialized();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        var unityAppApi = new BlocksOnGrid3DApp(this);
        unityAppApi.OnMessageError += (bytes, exception) =>
        {
            Logger.LogError($"Error deserializing message {bytes}", exception);
        };
        
        ParentApp.InitializeRenderer(unityAppApi);
        
        await base.OnAfterRenderAsync(firstRender);
    }
}