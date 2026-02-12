using RadioFreeDAM.App.Models;
using System.Text.Json;
using System.Diagnostics;

namespace RadioFreeDAM.App.Services;

public class SessionService
{
    private const string SessionKey = "user_session";
    public UserSession? CurrentUser { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            var sessionJson = await SecureStorage.GetAsync(SessionKey);
            if (!string.IsNullOrEmpty(sessionJson))
            {
                CurrentUser = JsonSerializer.Deserialize<UserSession>(sessionJson);
                Debug.WriteLine($"[Session] Restored user: {CurrentUser?.Username}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Session] Error initializing: {ex.Message}");
        }
    }

    public async Task SetUserAsync(UserSession? user)
    {
        CurrentUser = user;
        if (user != null)
        {
            var json = JsonSerializer.Serialize(user);
            await SecureStorage.SetAsync(SessionKey, json);
        }
        else
        {
            SecureStorage.Remove(SessionKey);
        }
    }

    public async Task SaveUserAsync(User? user)
    {
        if (user != null)
        {
            var session = new UserSession
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };
            await SetUserAsync(session);
        }
    }

    public bool IsLoggedIn => CurrentUser != null;

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        SecureStorage.Remove(SessionKey);
        await Shell.Current.GoToAsync("//login");
    }
}
