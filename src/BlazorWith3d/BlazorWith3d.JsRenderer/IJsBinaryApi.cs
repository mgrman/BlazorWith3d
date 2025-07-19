using BlazorWith3d.Shared;

using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.JsRenderer;

public interface IJsBinaryApi:IBinaryApi, IAsyncDisposable
{
    Task InitializeJsApp(string jsPath, ElementReference container, string initMethod = "InitializeApp",
        object? extraParam = null);
}