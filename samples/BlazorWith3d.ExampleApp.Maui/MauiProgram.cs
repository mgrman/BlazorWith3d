using Blazored.LocalStorage;

using BlazorWith3d.ExampleApp.Client.Services;

using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IFlagProvider,MauiFlagProvider>();
        builder.Services.AddBlazoredLocalStorage(c =>
        {
            c.JsonSerializerOptions.IncludeFields = true;
            c.JsonSerializerOptions.IgnoreReadOnlyFields = false;
            c.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
