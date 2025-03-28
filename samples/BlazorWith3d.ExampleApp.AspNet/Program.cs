using System.Buffers;

using Blazored.LocalStorage;
using BlazorWith3d.Components;
using BlazorWith3d.ExampleApp.Client.Services;
using BlazorWith3d.Unity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazoredLocalStorage(c =>
{
    c.JsonSerializerOptions.IncludeFields = true;
    c.JsonSerializerOptions.IgnoreReadOnlyFields = false;
    c.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
});

ArrayBufferWriter<byte> _writer = new ArrayBufferWriter<byte>(100);

var currentPos = _writer.WrittenCount;


builder.Services.AddSingleton<DebugRelayUnityApi>();
builder.Services.AddScoped<IFlagProvider,ServerFlagProvider>();
builder.Services.AddScoped<CookieStorageAccessor>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    var webSocketOptions = new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromMinutes(2)
    };

    app.UseWebSockets(webSocketOptions);

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/debug-relay-unity-ws")
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var debugRelay = context.RequestServices.GetRequiredService<DebugRelayUnityApi>();

                if (debugRelay.CanHandleWebSocket())
                {

                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await debugRelay.HandleWebSocket(webSocket);
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        else
        {
            await next(context);
        }
    });
}
app.UseHttpsRedirection();


app.UseAntiforgery();


app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
    context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
    context.Response.Headers.Append("Cross-Origin-Resource-Policy", "cross-origin");
    await next();
});


app.MapStaticAssets();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWith3d.ExampleApp.Client._Imports).Assembly)
    .AddAdditionalAssemblies(typeof(BlazorWith3d.ExampleApp.Client.Unity._Imports).Assembly);

app.Run();

