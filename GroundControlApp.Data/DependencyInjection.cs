using GroundControlApp.Data.Interfaces;
using GroundControlApp.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GroundControlApp.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddGroundControlAppData(
        this IServiceCollection services,
        Uri apiBaseAddress)
    {
        services.AddSingleton(_ => new HttpClient
        {
            BaseAddress = apiBaseAddress
        });
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<IIngredientService, IngredientService>();
        services.AddSingleton<IStockService, StockService>();
        services.AddSingleton<ITimeEntryService, TimeEntryService>();

        return services;
    }
}
