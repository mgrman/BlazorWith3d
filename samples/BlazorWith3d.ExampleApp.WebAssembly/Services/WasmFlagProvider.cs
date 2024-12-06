namespace BlazorWith3d.ExampleApp.Client.Services;

public class WasmFlagProvider: IFlagProvider
{
    public bool IsUnityRelayEnabled => false;

    public bool IsWindowsBuildLinkEnabled => true;
}