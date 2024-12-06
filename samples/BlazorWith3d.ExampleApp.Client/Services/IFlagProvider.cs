namespace BlazorWith3d.ExampleApp.Client.Services;

public interface IFlagProvider
{
    bool IsUnityRelayEnabled { get; }
    bool IsWindowsBuildLinkEnabled { get; }
}

