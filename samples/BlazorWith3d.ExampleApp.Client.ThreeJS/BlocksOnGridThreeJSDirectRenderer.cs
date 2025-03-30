using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.ExampleApp.Client.Shared.Blazor;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSDirectRenderer : BaseJsRenderer, IAsyncDisposable
{
    [CascadingParameter] 
    public required IBlocksOnGrid3DBlazorController ParentApp { get; set; }

    private DotNetObjectReference<BlocksOnGrid3DBlazorDirectBinding>? _messageReceiverProxyReference;
    
    [Inject]
    protected IJSRuntime _jsRuntime { get; set; } 

    [Inject] 
    protected ILogger<BlocksOnGridThreeJSDirectRenderer> _logger { get; set; }
    
    private string JsAppPath => Assets["./_content/BlazorWith3d.ExampleApp.Client.ThreeJS/clientassets/blazorwith3d-exampleapp-client-threejs-bundle.js"];

    private string InitializeMethodName => "InitializeApp_DirectInterop";
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            return;
        }

        await base.OnAfterRenderAsync(firstRender);
        
        var module = await _jsRuntime.LoadModuleAsync(JsAppPath);
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BlocksOnGrid3DBlazorDirectBinding());
        
        var app= await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference);
        _messageReceiverProxyReference.Value.SetTypescriptApp(app);
        _messageReceiverProxyReference.Value.SetEventHandler(ParentApp);
        
        await ParentApp.AddRenderer(new BlocksOnGrid3DBlazorRenderer(_messageReceiverProxyReference.Value, _containerElementReference));
    }

    public async ValueTask DisposeAsync()
    {
        if (_messageReceiverProxyReference != null)
        {
            await ParentApp.RemoveRenderer(_messageReceiverProxyReference.Value);
        }
        _messageReceiverProxyReference?.Dispose();
    }


}