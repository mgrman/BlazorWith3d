namespace BlazorWith3d.ExampleApp.Client.Services;

public class ServerFlagProvider: IFlagProvider
{
    public ServerFlagProvider(IWebHostEnvironment environment)
    {
        IsUnityRelayEnabled= environment.IsDevelopment();
    }
    
    public bool IsUnityRelayEnabled { get; private set; }

    public bool IsWindowsBuildLinkEnabled => true;
}