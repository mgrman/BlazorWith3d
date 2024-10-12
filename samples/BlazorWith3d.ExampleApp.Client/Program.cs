using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage(c =>
{
    c.JsonSerializerOptions.IncludeFields= true;
    c.JsonSerializerOptions.IgnoreReadOnlyFields = false;
    c.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
});

await builder.Build().RunAsync();

// TODO split project when second 3D backend is supported