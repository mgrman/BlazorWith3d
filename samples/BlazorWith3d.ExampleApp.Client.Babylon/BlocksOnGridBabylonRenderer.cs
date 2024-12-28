using BlazorWith3d.Babylon;
using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace BlazorWith3d.ExampleApp.Client.Babylon;

public class BlocksOnGridBabylonRenderer:BaseJsBinaryApiRenderer, IDisposable
{
    private BlocksOnGrid3DApp_BinaryApi? _unityAppApi;
    private IDisposable? _rendererAssignment;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }

    public override string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.Babylon/clientassets/blazorwith3d-exampleapp-client-babylon-bundle.js"];

    protected override string InitializeMethodName => "InitializeApp_BinaryApi";
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        _unityAppApi = new BlocksOnGrid3DApp_BinaryApi(this);
        _unityAppApi.OnMessageError += (bytes, exception) =>
        {
            Logger.LogError($"Error deserializing message {bytes}", exception);
        };
        
        _rendererAssignment=await ParentApp.InitializeRenderer(_unityAppApi, async () =>
        {
            await InitializeTypeScriptApp();
        });
    }

    public void Dispose()
    {
        _unityAppApi?.Dispose();
        _rendererAssignment?.Dispose();
    }
}