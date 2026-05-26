using GroundControlApp.Data;
using Microsoft.Extensions.Logging;

namespace GroundControlApp.Main
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddGroundControlAppData(GetApiBaseAddress());
            builder.Services.AddSingleton<Services.AppIdentitySession>();
            builder.Services.AddTransient<ViewModels.HomePageViewModel>();
            builder.Services.AddTransient<ViewModels.MainPageViewModel>();
            builder.Services.AddTransient<ViewModels.AdminPanelViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            AppServices.Services = app.Services;

            return app;
        }

        private static Uri GetApiBaseAddress()
        {
            return DeviceInfo.Platform == DevicePlatform.Android
                ? new Uri("http://10.0.2.2:5032/")
                : new Uri("http://localhost:5032/");
        }
    }
}
