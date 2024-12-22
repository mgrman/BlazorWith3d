using Blazored.LocalStorage;

using BlazorWith3d.ExampleApp.AspNet.WebAssembly;
using BlazorWith3d.ExampleApp.Client.Services;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage(c =>
{
    c.JsonSerializerOptions.IncludeFields = true;
    c.JsonSerializerOptions.IgnoreReadOnlyFields = false;
    c.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
});
builder.Services.AddSingleton<IFlagProvider, WasmFlagProvider>();
builder.Services.AddSingleton<CookieStorageAccessor>();

await builder.Build().RunAsync();

// TODO split project when second 3D backend is supported