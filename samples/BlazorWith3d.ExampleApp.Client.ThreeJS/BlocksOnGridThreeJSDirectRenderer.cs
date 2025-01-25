using BlazorWith3d.ExampleApp.Client.Shared;
using BlazorWith3d.ExampleApp.Client.Shared.Blazor;
using BlazorWith3d.JsApp;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.ExampleApp.Client.ThreeJS;

public class BlocksOnGridThreeJSDirectRenderer : BaseJsRenderer, IDisposable
{
    private IDisposable? _rendererAssignment;

    [CascadingParameter] 
    public required I3DAppController ParentApp { get; set; }

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
        
        _messageReceiverProxyReference = DotNetObjectReference.Create(new BlocksOnGrid3DBlazorDirectBinding());
        _rendererAssignment = await ParentApp.InitializeRenderer(_messageReceiverProxyReference.Value, async () =>
        {
            var module = await _jsRuntime.LoadModuleAsync(JsAppPath);
            var app= await module.InvokeAsync<IJSObjectReference>(InitializeMethodName, _containerElementReference,_messageReceiverProxyReference);
            _messageReceiverProxyReference.Value.SetTypescriptApp(app);
        });
    }

    public void Dispose()
    {
        _rendererAssignment?.Dispose();
        _messageReceiverProxyReference?.Dispose();
    }


}