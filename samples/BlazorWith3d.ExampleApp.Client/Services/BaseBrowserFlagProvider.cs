using BlazorWith3d.ExampleApp.Client.Services;

using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.ExampleApp.AspNet.WebAssembly;

public abstract class BaseBrowserFlagProvider : IFlagProvider
{
    public const string RenderModeCookieName = "renderMode";

    public static IComponentRenderMode[] SupportedRenderModes { get; }= [Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveServer, Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveWebAssembly, Microsoft.AspNetCore.Components.Web.RenderMode.InteractiveAuto];

    private readonly CookieStorageAccessor _cookieStorageAccessor;

    public BaseBrowserFlagProvider(CookieStorageAccessor cookieStorageAccessor)
    {
        _cookieStorageAccessor = cookieStorageAccessor;
    }

    public abstract bool IsUnityRelayEnabled { get; }

    public abstract bool IsWindowsBuildLinkEnabled { get; }

    IComponentRenderMode[] IFlagProvider.SupportedRenderModes => SupportedRenderModes;


    public async ValueTask<IComponentRenderMode?> GetRenderMode()
    {

        var renderModeName = await _cookieStorageAccessor.GetValueAsync<string>(RenderModeCookieName);
        return GetRenderModeForRequest(renderModeName);
    }
    public async ValueTask SetRenderMode(IComponentRenderMode? renderMode)
    {
         await _cookieStorageAccessor.SetValueAsync<string>(RenderModeCookieName, renderMode?.GetType().Name ?? "");
    }

    public static IComponentRenderMode GetRenderModeForRequest(string? cookieValue)
    {
        return SupportedRenderModes.FirstOrDefault(o => o.GetType().Name == cookieValue) ?? BaseBrowserFlagProvider.SupportedRenderModes.First();
    }
}