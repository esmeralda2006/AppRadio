using System.Net.Http.Json;
using System.Text.Json;
using RadioFreeDAM.App.Models;

namespace RadioFreeDAM.App.Services.Api;

public class ApiClient
{
    private readonly HttpClient _http;

    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(15); 
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        };
        System.Diagnostics.Debug.WriteLine("[ApiClient] Initialized with 15s timeout and CamelCase policy");
    }

    public async Task<List<Station>> GetStations()
    {
        try 
        {
            var stations = await _http.GetFromJsonAsync<List<Station>>("api/stations");
            return stations ?? new();
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert("Error de Conexión", "No se pudo conectar con el servidor.", "OK");
            return new();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting stations: {ex.Message}");
            return new();
        }
    }

    public async Task<List<Station>> SearchStations(string name)
    {
        try
        {
            var stations = await _http.GetFromJsonAsync<List<Station>>($"api/stations/search?name={Uri.EscapeDataString(name)}");
            return stations ?? new();
        }
        catch (HttpRequestException)
        {
            await Shell.Current.DisplayAlert("Error de Búsqueda", "Error al buscar emisoras.", "OK");
            return new();
        }
    }

    public async Task<List<Station>> GetStationsByCountry(string country)
    {
        try
        {
            var stations = await _http.GetFromJsonAsync<List<Station>>($"api/stations/country/{Uri.EscapeDataString(country)}");
            return stations ?? new();
        }
        catch (HttpRequestException)
        {
            await Shell.Current.DisplayAlert("Error de Filtro", "No se pudieron cargar las emisoras de este país.", "OK");
            return new();
        }
    }

    public async Task<bool> Register(string email, string password, string username = "Usuario")
    {
        try
        {
            var user = new { Email = email, PasswordHash = password, Username = username };
            var res = await _http.PostAsJsonAsync("api/auth/register", user);
            
            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                await Shell.Current.DisplayAlert("Registro Fallido", error, "OK");
                return false;
            }
            return true;
        }
        catch (HttpRequestException ex)
        {
            await Shell.Current.DisplayAlert("Error de Red", $"No se pudo conectar con el servidor: {ex.Message}", "OK");
            return false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Error inesperado: {ex.Message}", "OK");
            return false;
        }
    }

    public async Task<User?> Login(string username, string password)
    {
        System.Diagnostics.Debug.WriteLine($"[ApiClient] Starting Login request for {username}...");
        try
        {
            var loginData = new { Username = username, Password = password };
            System.Diagnostics.Debug.WriteLine($"[ApiClient] POST to api/auth/login...");
            var response = await _http.PostAsJsonAsync("api/auth/login", loginData);
            
            System.Diagnostics.Debug.WriteLine($"[ApiClient] Response received: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>(_jsonOptions);
                System.Diagnostics.Debug.WriteLine($"[ApiClient] User deserialized: {user?.Username ?? "NULL"}");
                return user;
            }
            
            var error = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[ApiClient] Login rejected by server: {error}");
            await Shell.Current.DisplayAlert("Login Fallido", error, "OK");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient] HTTP ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error de Conexión", $"No se pudo alcanzar el servidor (Check USB/Túnel): {ex.Message}", "OK");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient] UNEXPECTED ERROR: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", $"Fallo interno en el cliente: {ex.Message}", "OK");
            return null;
        }
    }

    public async Task<List<FavoriteEntity>> GetFavorites(int userId)
    {
        try
        {
            var favorites = await _http.GetFromJsonAsync<List<FavoriteEntity>>($"api/favorites/{userId}");
            return favorites ?? new();
        }
        catch
        {
            return new();
        }
    }

    // Overload without userId - uses current session
    public async Task<List<FavoriteEntity>> GetFavorites()
    {
        try
        {
            // Assuming the API can get favorites from the current authenticated user
            var favorites = await _http.GetFromJsonAsync<List<FavoriteEntity>>("api/favorites");
            return favorites ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task AddFavorite(FavoriteEntity fav)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/favorites", fav);
            if (!res.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo guardar en favoritos.", "OK");
            }
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo conectar con el servidor.", "OK");
        }
    }

    // Standard method using Station and UserId
    public async Task AddFavorite(Station station, int userId)
    {
        try
        {
            var fav = new FavoriteEntity
            {
                UserId = userId,
                StationId = station.Id,
                Name = station.Name,
                StreamUrl = station.Url,
                ImageUrl = station.ImageUrl,
                Genre = station.Genre
            };
            
            var res = await _http.PostAsJsonAsync("api/favorites", fav);
            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                await Shell.Current.DisplayAlert("Error de Favoritos", $"No se pudo guardar: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] Error adding favorite: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo conectar con el servidor.", "OK");
        }
    }
    
    public async Task DeleteFavorite(int id)
    {
        try
        {
            await _http.DeleteAsync($"api/favorites/{id}");
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar de favoritos.", "OK");
        }
    }

    // Alias for RemoveFavorite
    public async Task RemoveFavorite(int userId, string stationId)
    {
        try
        {
            var res = await _http.DeleteAsync($"api/favorites/{userId}/{stationId}");
            if (!res.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo eliminar de favoritos.", "OK");
            }
        }
        catch
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar de favoritos.", "OK");
        }
    }
    
    public async Task<int> SyncStations()
    {
        try
        {
            var res = await _http.PostAsync("api/sync/radiobrowser", null);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
                return json?["count"]?.GetValue<int>() ?? 0;
            }
            return 0;
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error de Sincronización", "Error al conectar con la base de datos externa.", "OK");
            return 0;
        }
    }

    // Alias for SyncRadios
    public async Task SyncRadios()
    {
        await SyncStations();
    }
}
