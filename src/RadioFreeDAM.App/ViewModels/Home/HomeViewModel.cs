using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadioFreeDAM.App.Models;
using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.Services;
using RadioFreeDAM.App.Services.Player;
using RadioFreeDAM.App.ViewModels.Base;
using System.Collections.ObjectModel;

namespace RadioFreeDAM.App.ViewModels.Home;

public partial class HomeViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;
    private readonly RadioPlayerService _player;

    private ObservableCollection<Station> stations = new();
    public ObservableCollection<Station> Stations
    {
        get => stations;
        set => SetProperty(ref stations, value);
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    private string searchQuery = string.Empty;
    public string SearchQuery
    {
        get => searchQuery;
        set => SetProperty(ref searchQuery, value);
    }

    public IAsyncRelayCommand LoadStationsCommand { get; }
    public IAsyncRelayCommand SyncStationsCommand { get; }
    public IAsyncRelayCommand<Station> AddFavoriteCommand { get; }
    public IAsyncRelayCommand<Station> PlayCommand { get; }
    public IAsyncRelayCommand GoToFavoritesCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand LoadSpainRadiosCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    public HomeViewModel(ApiClient api, SessionService session, RadioPlayerService player)
    {
        _api = api;
        _session = session;
        _player = player;

        LoadStationsCommand = new AsyncRelayCommand(LoadStations);
        SyncStationsCommand = new AsyncRelayCommand(Sync);
        AddFavoriteCommand = new AsyncRelayCommand<Station>(AddFavorite);
        PlayCommand = new AsyncRelayCommand<Station>(PlayAction);
        GoToFavoritesCommand = new AsyncRelayCommand(GoToFavorites);
        SearchCommand = new AsyncRelayCommand(Search);
        LoadSpainRadiosCommand = new AsyncRelayCommand(LoadSpainRadios);
        LogoutCommand = new AsyncRelayCommand(Logout);
    }

    private async Task PlayAction(Station? station)
    {
        if (station == null) return;

        try
        {
            var success = await _player.PlayAsync(station);

            if (success)
            {
                // Navegar a la página del reproductor
                await Shell.Current.GoToAsync("//main/player");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error en PlayAction: {ex.Message}");
            
            await Shell.Current.DisplayAlert(
                "Error",
                "No se pudo reproducir la emisora. Intenta de nuevo.",
                "OK"
            );
        }
    }

    private async Task Logout()
    {
        try
        {
            await _session.LogoutAsync();
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error en Logout: {ex.Message}");
        }
    }

    private async Task LoadStations()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var userId = _session.CurrentUser?.Id ?? 0;
            var dataTask = _api.GetStations();
            var favoritesTask = _api.GetFavorites(userId);

            await Task.WhenAll(dataTask, favoritesTask);

            var data = await dataTask;
            var favorites = await favoritesTask;

            Stations.Clear();
            foreach (var station in data)
            {
                station.IsFavorite = favorites.Any(f => f.StationId == station.Id);
                Stations.Add(station);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error cargando emisoras: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudieron cargar las emisoras.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddFavorite(Station? station)
    {
        if (station == null) return;

        try
        {
            // Immediate UI feedback
            station.IsFavorite = !station.IsFavorite;
            
            var userId = _session.CurrentUser?.Id ?? 0;
            if (station.IsFavorite)
            {
                await _api.AddFavorite(station, userId);
            }
            else
            {
                await _api.RemoveFavorite(userId, station.Id);
            }
        }
        catch (Exception ex)
        {
            // Rollback on error
            station.IsFavorite = !station.IsFavorite;
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error toggling favorite: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo actualizar favoritos.", "OK");
        }
        finally
        {
            // Force UI update if binding doesn't catch it
            OnPropertyChanged(nameof(Stations));
        }
    }

    private async Task Sync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            await _api.SyncRadios();
            await LoadStations();
            
            await Shell.Current.DisplayAlert(
                "Sincronización Completa",
                "Las emisoras se han actualizado correctamente.",
                "OK"
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error en sincronización: {ex.Message}");
            await Shell.Current.DisplayAlert("Error", "No se pudo sincronizar.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task Search()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var favorites = await _api.GetFavorites();
            var data = string.IsNullOrWhiteSpace(SearchQuery)
                ? await _api.GetStations()
                : await _api.SearchStations(SearchQuery);
            
            Stations.Clear();
            foreach (var station in data)
            {
                station.IsFavorite = favorites.Any(f => f.StationId == station.Id);
                Stations.Add(station);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error en búsqueda: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSpainRadios()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var favorites = await _api.GetFavorites();
            var data = await _api.GetStationsByCountry("Spain");
            
            Stations.Clear();
            foreach (var station in data)
            {
                station.IsFavorite = favorites.Any(f => f.StationId == station.Id);
                Stations.Add(station);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Error cargando radios de España: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GoToFavorites()
    {
        await Shell.Current.GoToAsync("//main/favorites");
    }
}
