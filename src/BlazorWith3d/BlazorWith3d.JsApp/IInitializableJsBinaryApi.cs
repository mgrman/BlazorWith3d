using Microsoft.AspNetCore.Components;

namespace BlazorWith3d.JsApp;

public interface IInitializableJsBinaryApi: IAsyncDisposable
{
    Task InitializeJsApp(string jsPath, ElementReference container, string initMethod = "InitializeApp",
        object? extraParam = null);
}