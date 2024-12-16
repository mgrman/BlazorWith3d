using BlazorWith3d.ExampleApp.Client.Services;

namespace BlazorWith3d.ExampleApp.AspNet.WebAssembly;

public class WasmFlagProvider: IFlagProvider
{
    public bool IsUnityRelayEnabled => false;

    public bool IsWindowsBuildLinkEnabled => true;
}