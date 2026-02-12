using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RadioFreeDAM.App.ViewModels.Auth;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    
    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private bool isPasswordMasked = true;

    [ObservableProperty]
    private bool isLoading;

    public RegisterViewModel(ApiClient api)
    {
        _api = api;
        System.Diagnostics.Debug.WriteLine("[RegisterViewModel] Initialized");
    }

    [RelayCommand]
    private async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void TogglePassword() => IsPasswordMasked = !IsPasswordMasked;

    [RelayCommand]
    private async Task Register()
    {
        System.Diagnostics.Debug.WriteLine($"[RegisterViewModel] Registering {Email}...");
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Username))
        {
            await Shell.Current.DisplayAlert("Campos requeridos", "Por favor, rellena todos los campos.", "OK");
            return;
        }

        if (!IsValidEmail(Email))
        {
            await Shell.Current.DisplayAlert("Email inválido", "Por favor, introduce un correo electrónico válido.", "OK");
            return;
        }

        if (Password.Length < 6)
        {
            await Shell.Current.DisplayAlert("Contraseña corta", "La contraseña debe tener al menos 6 caracteres.", "OK");
            return;
        }

        IsLoading = true;
        try 
        {
            System.Diagnostics.Debug.WriteLine("[RegisterViewModel] Calling API Register...");
            var ok = await _api.Register(Email, Password, Username ?? "Usuario");

            if (ok)
            {
                System.Diagnostics.Debug.WriteLine("[RegisterViewModel] Register success");
                await Shell.Current.DisplayAlert(
                    "Correcto",
                    "Usuario registrado correctamente",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RegisterViewModel] CRITICAL ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Red", "No se pudo completar el registro. Verifica tu conexión.", "OK");
        }
        finally 
        {
            IsLoading = false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
