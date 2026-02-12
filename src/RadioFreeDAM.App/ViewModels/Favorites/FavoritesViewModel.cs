using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadioFreeDAM.App.Models;
using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.Services;
using RadioFreeDAM.App.Services.Player;
using RadioFreeDAM.App.ViewModels.Base;
using System.Collections.ObjectModel;

namespace RadioFreeDAM.App.ViewModels.Favorites;

public partial class FavoritesViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;
    private readonly RadioPlayerService _player;

    private ObservableCollection<FavoriteEntity> favorites = new();
    public ObservableCollection<FavoriteEntity> Favorites
    {
        get => favorites;
        set => SetProperty(ref favorites, value);
    }

    private bool isLoading;
    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public IAsyncRelayCommand LoadFavoritesCommand { get; }
    public IAsyncRelayCommand<FavoriteEntity> PlayCommand { get; }
    public IAsyncRelayCommand<FavoriteEntity> RemoveFavoriteCommand { get; }

    public FavoritesViewModel(ApiClient api, SessionService session, RadioPlayerService player)
    {
        _api = api;
        _session = session;
        _player = player;

        LoadFavoritesCommand = new AsyncRelayCommand(LoadFavorites);
        PlayCommand = new AsyncRelayCommand<FavoriteEntity>(PlayAction);
        RemoveFavoriteCommand = new AsyncRelayCommand<FavoriteEntity>(RemoveFavorite);
    }

    private async Task PlayAction(FavoriteEntity? fav)
    {
        if (fav == null) return;

        try
        {
            // Convertir FavoriteEntity a Station
            var station = new Station 
            { 
                Id = fav.StationId, 
                Name = fav.Name, 
                Url = fav.StreamUrl, 
                ImageUrl = fav.ImageUrl, 
                Genre = fav.Genre 
            };

            var success = await _player.PlayAsync(station);

            if (success)
            {
                await Shell.Current.GoToAsync("//main/player");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoritesViewModel] Error en PlayAction: {ex.Message}");
            
            await Shell.Current.DisplayAlert(
                "Error",
                "No se pudo reproducir la emisora.",
                "OK"
            );
        }
    }

    private async Task LoadFavorites()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            Favorites.Clear();
            var userId = _session.CurrentUser?.Id ?? 0;
            var data = await _api.GetFavorites(userId);
            
            foreach (var fav in data)
            {
                Favorites.Add(fav);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoritesViewModel] Error cargando favoritos: {ex.Message}");
            
            await Shell.Current.DisplayAlert(
                "Error",
                "No se pudieron cargar los favoritos.",
                "OK"
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RemoveFavorite(FavoriteEntity? fav)
    {
        if (fav == null) return;

        try
        {
            var userId = _session.CurrentUser?.Id ?? 0;
            await _api.RemoveFavorite(userId, fav.StationId);
            Favorites.Remove(fav);
            
            await Shell.Current.DisplayAlert(
                "Favorito Eliminado",
                $"{fav.Name} se eliminó de tus favoritos.",
                "OK"
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoritesViewModel] Error eliminando favorito: {ex.Message}");
            
            await Shell.Current.DisplayAlert(
                "Error",
                "No se pudo eliminar el favorito.",
                "OK"
            );
        }
    }
}
