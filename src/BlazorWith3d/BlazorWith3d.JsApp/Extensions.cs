using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BlazorWith3d.JsApp;

public static class Extensions
{
    public static async Task<IJSObjectReference> LoadModuleAsync(this IJSRuntime jsRuntime, string jsPath)
    {
        var moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync
            <IJSObjectReference>
            ("import", jsPath).AsTask());
        
        return await moduleTask.Value;
    }

    public static async Task TryInvokeVoidAsync(this IJSObjectReference jsObject, ILogger loggerIfError, string method, params object[] args)
    {
        try
        {
            await jsObject.InvokeVoidAsync(method, args);
        }
        catch (Exception ex)
        {
            loggerIfError.LogError(ex,"BaseJsApi DisposeAsync failed to invoke Quit method");
        }
    }

    public static async Task TryDisposeAsync(this IAsyncDisposable? obj)
    {
        if (obj == null)
        {
            return;
        }
        await obj.DisposeAsync();
    }
}