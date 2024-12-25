using BlazorWith3d.Babylon;
using BlazorWith3d.ExampleApp.Client.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJs;

public class BlocksOnGridThreeJSDirectRenderer:BaseJsRenderer, IDisposable, IBlocksOnGrid3DApp
{
    private BlocksOnGrid3DApp? unityAppApi;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }
    
    [Inject]
    public required ILogger<BlocksOnGridThreeJSDirectRenderer> Logger { get; set; }

    public override string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS.DirectInterop/clientassets/blazorwith3d-exampleapp-client-threejs-direct-bundle.js"];

    protected override JsMessageReceiverProxy CreateReceiverProxy(Action<byte[]> callback)
    {
        return new ExtendedJsMessageReceiverProxy(this,callback);
    }


    protected class ExtendedJsMessageReceiverProxy(
        BlocksOnGridThreeJSDirectRenderer app,
        Action<byte[]> onMessageBytesReceived)
        : JsMessageReceiverProxy(onMessageBytesReceived)
    {
        private readonly BlocksOnGridThreeJSDirectRenderer _app = app;


        [JSInvokable]
        public void InvokePerfCheck(PerfCheck msg)
        {
            app.OnPerfCheck?.Invoke(msg);

        }

        [JSInvokable]
        public void InvokeUnityAppInitialized(UnityAppInitialized msg)
        {

            app.OnUnityAppInitialized?.Invoke(msg);
        }

        [JSInvokable]
        public void InvokeRaycastResponse(RaycastResponse msg)
        {
            app.OnRaycastResponse?.Invoke(msg);
        }

        [JSInvokable]
        public void InvokeScreenToWorldRayResponse(ScreenToWorldRayResponse msg)
        {
            app.OnScreenToWorldRayResponse?.Invoke(msg);
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
        if (unityAppApi != null)
        {
            ParentApp.InitializeRenderer(null);
            unityAppApi.Dispose();
        }
    }

    public bool IsProcessingMessages { get; }
    public void StartProcessingMessages()
    {
        //TODO
    }

    public void StopProcessingMessages()
    {
        //TODO
    }

    //TODO
    public event Action<byte[], Exception>? OnMessageError;
    
    
    public event Action<PerfCheck>? OnPerfCheck;
    public event Action<UnityAppInitialized>? OnUnityAppInitialized;
    public event Action<RaycastResponse>? OnRaycastResponse;
    public event Action<ScreenToWorldRayResponse>? OnScreenToWorldRayResponse;
    public async ValueTask InvokeBlazorControllerInitialized(BlazorControllerInitialized msg)
    {
      await  _typescriptApp.InvokeVoidAsync($"On{nameof(BlazorControllerInitialized)}", msg);
    }

    public async ValueTask InvokePerfCheck(PerfCheck msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(PerfCheck)}", msg);
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

    public async ValueTask InvokeRequestRaycast(RequestRaycast msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(RequestRaycast)}", msg);
    }

    public async ValueTask InvokeRequestScreenToWorldRay(RequestScreenToWorldRay msg)
    {
        await  _typescriptApp.InvokeVoidAsync($"On{nameof(RequestScreenToWorldRay)}", msg);
    }
}