using RadioFreeDAM.App.Views.Auth;
using RadioFreeDAM.App.Views.Favorites;
using RadioFreeDAM.App.Views.Home;
using RadioFreeDAM.App.Views.Player;
using RadioFreeDAM.App.Services.Player;
using CommunityToolkit.Mvvm.Input;

namespace RadioFreeDAM.App;

public partial class AppShell : Shell
{
    private readonly RadioPlayerService _playerService;
    public RadioPlayerService Player => _playerService;
    public IRelayCommand GoToPlayerCommand { get; }

    public AppShell(RadioPlayerService playerService)
    {
        _playerService = playerService;
        GoToPlayerCommand = new RelayCommand(async () => await GoToAsync("//player"));
        
        InitializeComponent();
        
        // Conectar el MediaElement global al servicio de reproducción
        _playerService.Initialize(GlobalPlayerInstance);
        
        // Establecer el BindingContext para la barra de reproducción
        BindingContext = this;

        // Registrar rutas de navegación
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Limpiar recursos del reproductor al cerrar la app
        _playerService?.Cleanup();
    }
}
