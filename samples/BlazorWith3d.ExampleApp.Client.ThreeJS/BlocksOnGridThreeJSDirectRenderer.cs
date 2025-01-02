using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.ExampleApp.Client.Shared.Blazor;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSDirectRenderer : BaseJsRenderer, IDisposable
{
    private IBlocksOnGrid3DApp_EventHandler? _eventHandler;
    private IDisposable? _rendererAssignment;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }

    private DotNetObjectReference<BlocksOnGrid3DBlazorDirectBinding>? _messageReceiverProxyReference;
    
    public override string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS/clientassets/blazorwith3d-exampleapp-client-threejs-bundle.js"];

    protected string InitializeMethodName => "InitializeApp_DirectInterop";
    
    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module)
    {
        var app= await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference);
        _messageReceiverProxyReference.Value.SetTypescriptApp(app);
        
        return app;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        await base.OnAfterRenderAsync(firstRender);
        
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BlocksOnGrid3DBlazorDirectBinding());
        _rendererAssignment = await ParentApp.InitializeRenderer(_messageReceiverProxyReference.Value, async () =>
        {
            await InitializeTypeScriptApp();
        });
    }

    public void Dispose()
    {
        _rendererAssignment?.Dispose();
        _messageReceiverProxyReference?.Dispose();
    }


}