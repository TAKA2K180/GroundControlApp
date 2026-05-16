using Microsoft.Extensions.DependencyInjection;

namespace GroundControlApp.Main;

public static class AppServices
{
    public static IServiceProvider Services { get; set; } = null!;

    public static T GetRequiredService<T>()
        where T : notnull
    {
        return Services.GetRequiredService<T>();
    }
}
