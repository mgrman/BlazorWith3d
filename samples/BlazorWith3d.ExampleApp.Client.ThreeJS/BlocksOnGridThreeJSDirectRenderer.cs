using BlazorWith3d.Babylon;
using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSDirectRenderer : BaseJsRenderer, IDisposable, IBlocksOnGrid3DApp
{
    private IBlocksOnGrid3DApp_EventHandler? _eventHandler;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    public override string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS/clientassets/blazorwith3d-exampleapp-client-threejs-bundle.js"];

    protected string InitializeMethodName => "InitializeApp_DirectInterop";
    
    protected override async Task<IJSObjectReference?> InitializeJsApp(IJSObjectReference module, DotNetObjectReference<JsMessageReceiverProxy> messageReceiverProxyReference)
    { 
        return await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,messageReceiverProxyReference);
    }

    protected override JsMessageReceiverProxy CreateReceiverProxy()
    {
        return new ExtendedJsMessageReceiverProxy(this);
    }

    protected class ExtendedJsMessageReceiverProxy(
        BlocksOnGridThreeJSDirectRenderer app)
        : JsMessageReceiverProxy(),  IBlocksOnGrid3DApp_EventHandler
    {

        [JSInvokable]
        public async ValueTask OnUnityAppInitialized(UnityAppInitialized msg)
        {
            app._eventHandler?.OnUnityAppInitialized(msg);
        }

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }
        await base.OnAfterRenderAsync(firstRender);
        
        ParentApp.InitializeRenderer(this);
    }

    public void Dispose()
    {
        ParentApp.InitializeRenderer(null);
    }

    //TODO
    public event Action<byte[], Exception>? OnMessageError;
    public void SetEventHandler(IBlocksOnGrid3DApp_EventHandler? eventHandler)
    {
        _eventHandler=eventHandler;
        
    }

    public async ValueTask InvokeBlazorControllerInitialized(BlazorControllerInitialized msg)
    {
      await  _typescriptApp.InvokeVoidAsync($"On{nameof(BlazorControllerInitialized)}", msg);
    }

    public async ValueTask<PerfCheck> InvokePerfCheck(PerfCheck msg)
    {
      return  await  _typescriptApp.InvokeAsync<PerfCheck>($"On{nameof(PerfCheck)}", msg);
    }

    public async ValueTask InvokeAddBlockTemplate(AddBlockTemplate msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(AddBlockTemplate)}", msg);
    }

    public async ValueTask InvokeAddBlockInstance(AddBlockInstance msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(AddBlockInstance)}", msg);
    }

    public async ValueTask InvokeRemoveBlockInstance(RemoveBlockInstance msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(RemoveBlockInstance)}", msg);
    }

    public async ValueTask InvokeRemoveBlockTemplate(RemoveBlockTemplate msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(RemoveBlockTemplate)}", msg);
    }

    public async ValueTask InvokeUpdateBlockInstance(UpdateBlockInstance msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(UpdateBlockInstance)}", msg);
    }

    public async ValueTask<RaycastResponse> InvokeRequestRaycast(RequestRaycast msg)
    {
       return await  _typescriptApp.InvokeAsync<RaycastResponse>($"On{nameof(RequestRaycast)}", msg);
    }

    public async ValueTask<ScreenToWorldRayResponse> InvokeRequestScreenToWorldRay(RequestScreenToWorldRay msg)
    {
      return  await  _typescriptApp.InvokeAsync<ScreenToWorldRayResponse>($"On{nameof(RequestScreenToWorldRay)}", msg);
    }
}