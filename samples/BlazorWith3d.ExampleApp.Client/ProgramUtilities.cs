using Microsoft.Extensions.DependencyInjection;

namespace BlazorWith3d.ExampleApp.Client;

public static class ProgramUtilities
{
    public static void InitializeClientServices(this IServiceCollection services)
    {
        services.AddBlazorContextMenu();
    }
}