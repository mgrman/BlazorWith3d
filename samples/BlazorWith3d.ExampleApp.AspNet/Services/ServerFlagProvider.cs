
using BlazorWith3d.ExampleApp.AspNet.WebAssembly;

using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.ExampleApp.Client.Services;

public class ServerFlagProvider: BaseBrowserFlagProvider
{
    public ServerFlagProvider(IWebHostEnvironment environment, CookieStorageAccessor cookieStorageAccessor)
        :base(cookieStorageAccessor)
    {
        _isUnityRelayEnabled = environment.IsDevelopment();
    }

    private readonly bool _isUnityRelayEnabled;


    public override bool IsUnityRelayEnabled => _isUnityRelayEnabled;

    public override bool IsWindowsBuildLinkEnabled => true;

}