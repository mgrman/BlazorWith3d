using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.ExampleApp.Client.Services;

public interface IFlagProvider
{
    bool IsUnityRelayEnabled { get; }
    bool IsWindowsBuildLinkEnabled { get; }

    IComponentRenderMode[] SupportedRenderModes { get; }

    ValueTask<IComponentRenderMode?> GetRenderMode();
     ValueTask SetRenderMode(IComponentRenderMode? renderMode);
}

