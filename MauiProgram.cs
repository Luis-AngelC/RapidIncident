using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using IncidentReportApp.Services;
using IncidentReportApp.ViewModels;
using IncidentReportApp.Views;

namespace IncidentReportApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

                .ConfigureMauiHandlers(handlers =>
                {
               
                 });


            // Registrar Servicios
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddTransient<CameraService>();
            builder.Services.AddTransient<LocationService>();

            // Registrar ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<CreateIncidentViewModel>();
            builder.Services.AddTransient<IncidentListViewModel>();
            builder.Services.AddTransient<IncidentDetailViewModel>();

            // Registrar Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<CreateIncidentPage>();
            builder.Services.AddTransient<IncidentListPage>();
            builder.Services.AddTransient<IncidentDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}