
using BlazorWith3d.ExampleApp.Client.Services;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace MauiApp1;

internal class MauiFlagProvider : IFlagProvider
{
    public bool IsUnityRelayEnabled => false;

    public bool IsWindowsBuildLinkEnabled => false;

    public IComponentRenderMode[] SupportedRenderModes => Array.Empty<IComponentRenderMode>();

    public ValueTask<IComponentRenderMode?> GetRenderMode()
    {
        return ValueTask.FromResult<IComponentRenderMode?>(null);
    }

    public ValueTask SetRenderMode(IComponentRenderMode? renderMode)
    {
        throw new NotImplementedException();
    }
}