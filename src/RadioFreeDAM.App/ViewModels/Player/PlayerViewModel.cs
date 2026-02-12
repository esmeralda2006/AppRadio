using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadioFreeDAM.App.Models;
using RadioFreeDAM.App.Services.Api;
using RadioFreeDAM.App.Services.Player;
using System.Collections.ObjectModel;

namespace RadioFreeDAM.App.ViewModels.Player;

public partial class PlayerViewModel : ObservableObject
{
    private readonly ApiClient _api;
    public RadioPlayerService Player { get; }

    [ObservableProperty]
    private ObservableCollection<Station> radios = new();

    public IAsyncRelayCommand LoadRadiosCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand TogglePlaybackCommand { get; }

    public PlayerViewModel(ApiClient api, RadioPlayerService player)
    {
        _api = api;
        Player = player;

        LoadRadiosCommand = new AsyncRelayCommand(LoadRadios);
        StopCommand = new RelayCommand(Stop);
        TogglePlaybackCommand = new RelayCommand(TogglePlayback);
    }

    private async Task LoadRadios()
    {
        try
        {
            Player.StatusMessage = "Cargando emisoras...";
            Radios.Clear();
            
            var data = await _api.GetStations();
            
            foreach (var station in data)
            {
                Radios.Add(station);
            }
            
            Player.StatusMessage = "";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PlayerViewModel] Error cargando radios: {ex.Message}");
            Player.StatusMessage = "Error cargando emisoras";
        }
    }

    private void Stop()
    {
        Player.Stop();
    }

    private void TogglePlayback()
    {
        Player.TogglePlayback();
    }
}
