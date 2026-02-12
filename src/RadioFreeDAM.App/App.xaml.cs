using RadioFreeDAM.App.Models;
using RadioFreeDAM.App.Services;
using System.Diagnostics;

namespace RadioFreeDAM.App;

public partial class App : Application
{
    private readonly SessionService _sessionService;

    public App(SessionService sessionService, IServiceProvider services)
    {
        InitializeComponent();
        _sessionService = sessionService;
        
        // Configurar manejo global de excepciones
        ConfigureGlobalExceptionHandling();
        
        MainPage = services.GetRequiredService<AppShell>();
    }

    protected override async void OnStart()
    {
        try
        {
            await _sessionService.InitializeAsync();
            
            if (_sessionService.CurrentUser != null)
            {
                Debug.WriteLine($"[App] Usuario autenticado: {_sessionService.CurrentUser.Username}");
                
                await MainThread.InvokeOnMainThreadAsync(async () => 
                {
                    // Navegar al TabBar principal
                    await Shell.Current.GoToAsync("//main/home");
                });
            }
            else
            {
                Debug.WriteLine("[App] No hay sesión activa, mostrando login");
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Error durante inicio: {ex.Message}");
            await Shell.Current.GoToAsync("//login");
        }
    }

    private void ConfigureGlobalExceptionHandling()
    {
        // Capturar excepciones no manejadas en el dominio de la aplicación
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                HandleException(ex, "AppDomain.UnhandledException");
            }
        };

        // Capturar excepciones no observadas en tareas
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            HandleException(args.Exception, "TaskScheduler.UnobservedTaskException");
            args.SetObserved(); // Prevenir que la app se cierre
        };
    }

    public static void HandleException(Exception ex, string source = "Unknown")
    {
        Debug.WriteLine($"[GLOBAL EXCEPTION] Source: {source}");
        Debug.WriteLine($"[GLOBAL EXCEPTION] Message: {ex.Message}");
        Debug.WriteLine($"[GLOBAL EXCEPTION] StackTrace: {ex.StackTrace}");
        
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error Inesperado",
                        "Ha ocurrido un error en la aplicación. Por favor, intenta de nuevo.\n\n" +
                        $"Detalles técnicos: {ex.Message}",
                        "OK"
                    );
                }
            }
            catch (Exception alertEx)
            {
                Debug.WriteLine($"[GLOBAL EXCEPTION] No se pudo mostrar alerta: {alertEx.Message}");
            }
        });
    }
}
