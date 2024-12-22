using BlazorWith3d.ExampleApp.Client.Services;

using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.ExampleApp.AspNet.WebAssembly;

public class WasmFlagProvider: BaseBrowserFlagProvider
{
    public WasmFlagProvider(CookieStorageAccessor cookieStorageAccessor)
        :base(cookieStorageAccessor)
    {
    }

    public override bool IsUnityRelayEnabled => false;

    public override bool IsWindowsBuildLinkEnabled => true;

}