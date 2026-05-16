using GroundControlApp.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GroundControlApp.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddGroundControlAppData(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        services.AddSingleton<IGroundControlApiClient>(_ =>
            new GroundControlApiClient(new HttpClient
            {
                BaseAddress = apiBaseAddress
            }));

        return services;
    }
}
