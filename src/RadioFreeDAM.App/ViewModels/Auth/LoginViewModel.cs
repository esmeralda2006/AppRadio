using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadioFreeDAM.App.Services;
using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.ViewModels.Base;

namespace RadioFreeDAM.App.ViewModels.Auth;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isPasswordMasked = true;

    // Commands are now handled by source generators via [RelayCommand] attributes below.

    public LoginViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
        System.Diagnostics.Debug.WriteLine("[LoginViewModel] Initialized");
    }

    [RelayCommand]
    private async Task Login()
    {
        System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login attempted for: {Username}");
        
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Error", "Por favor completa todos los campos", "OK");
            return;
        }

        IsLoading = true;
        try
        {
            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Calling API...");
            var user = await _api.Login(Username, Password);
            
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Login success: {user.Username}");
                await _session.SaveUserAsync(user);
                await Shell.Current.GoToAsync("//main/home");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Login failed: User is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoginViewModel] CRITICAL ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Red", "No se pudo conectar con el servidor. Verifica tu conexión.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegister()
    {
        System.Diagnostics.Debug.WriteLine("[LoginViewModel] Navigating to Register");
        await Shell.Current.GoToAsync("RegisterPage");
    }

    [RelayCommand]
    private void TogglePassword()
    {
        IsPasswordMasked = !IsPasswordMasked;
    }
}
