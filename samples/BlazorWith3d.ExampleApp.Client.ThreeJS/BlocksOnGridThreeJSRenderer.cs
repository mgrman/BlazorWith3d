using BlazorWith3d.Babylon;
using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSRenderer: BaseJsBinaryApiRenderer, IDisposable
{
    private BlocksOnGrid3DApp_BinaryApi? unityAppApi;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    public override string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS/clientassets/blazorwith3d-exampleapp-client-threejs-bundle.js"];

    protected override string InitializeMethodName => "InitializeApp_BinaryApi";
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