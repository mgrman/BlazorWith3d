using BlazorWith3d.Babylon;
using BlazorWith3d.ExampleApp.Client.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.Babylon;

public class BlocksOnGridBabylonRenderer:BaseBabylonRenderer, IDisposable
{
    private BlocksOnGrid3DApp? unityAppApi;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    [Inject]
    public required ILogger<BlocksOnGridBabylonRenderer> Logger { get; set; }

    public override string BabylonAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Babylon/clientassets/blazorwith3d-exampleapp-client-babylon-bundle.js"];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        unityAppApi = new BlocksOnGrid3DApp(this);
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