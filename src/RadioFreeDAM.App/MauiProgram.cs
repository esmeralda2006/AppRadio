using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using RadioFreeDAM.App.Services;
using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.ViewModels.Auth;
using RadioFreeDAM.App.ViewModels.Home;
using RadioFreeDAM.App.ViewModels.Player;
using RadioFreeDAM.App.Views.Auth;
using RadioFreeDAM.App.Views.Home;
using RadioFreeDAM.App.Views.Player;
using RadioFreeDAM.App.Views.Favorites;
using RadioFreeDAM.App.ViewModels.Favorites;
using RadioFreeDAM.App.Services.Player;

namespace RadioFreeDAM.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement() // Initialize MediaElement
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Services
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<RadioPlayerService>();
        
        builder.Services.AddHttpClient<ApiClient>(client => 
        {
            // Using adb reverse for Android, so localhost works on both
            client.BaseAddress = new Uri("http://localhost:5156"); 
        });

        builder.Services.AddSingleton<AppShell>();

        // Register ViewModels and Pages automatically (cleaner approach)
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<PlayerViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<PlayerPage>();
        builder.Services.AddTransient<FavoritesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Global Generic Exception Handling
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            System.Diagnostics.Debug.WriteLine($"[Global Error] Unhandled Exception: {exception.Message}");
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            System.Diagnostics.Debug.WriteLine($"[Global Error] Unobserved Task Exception: {args.Exception.Message}");
            args.SetObserved();
        };

        return builder.Build();
    }
}
