using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using RadioFreeDAM.App.Models;
using System.Diagnostics;

namespace RadioFreeDAM.App.Services.Player;

public partial class RadioPlayerService : ObservableObject
{
    private MediaElement? _mediaElement;

    [ObservableProperty]
    private Station? currentStation;

    [ObservableProperty]
    private bool isPlaying;

    [ObservableProperty]
    private bool isBuffering;

    [ObservableProperty]
    private string statusMessage = "Listo";

    public IAsyncRelayCommand<Station> PlayCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand TogglePlaybackCommand { get; }

    public RadioPlayerService()
    {
        PlayCommand = new AsyncRelayCommand<Station>(PlayAsync);
        StopCommand = new RelayCommand(Stop);
        TogglePlaybackCommand = new RelayCommand(TogglePlayback);
    }

    public void Initialize(MediaElement mediaElement)
    {
        if (_mediaElement != null)
        {
            // Limpiar eventos anteriores
            _mediaElement.MediaOpened -= OnMediaOpened;
            _mediaElement.MediaFailed -= OnMediaFailed;
            _mediaElement.StateChanged -= OnStateChanged;
        }

        _mediaElement = mediaElement;
        
        if (_mediaElement != null)
        {
            _mediaElement.MediaOpened += OnMediaOpened;
            _mediaElement.MediaFailed += OnMediaFailed;
            _mediaElement.StateChanged += OnStateChanged;
        }
    }

    public async Task<bool> PlayAsync(Station? station)
    {
        try
        {
            if (station == null)
            {
                StatusMessage = "Emisora no válida";
                return false;
            }

            if (string.IsNullOrWhiteSpace(station.Url))
            {
                StatusMessage = "URL de streaming no disponible";
                await ShowErrorAlert("Esta emisora no tiene URL de streaming válida.");
                return false;
            }

            // Validar formato de URL
            if (!Uri.TryCreate(station.Url, UriKind.Absolute, out var uri))
            {
                StatusMessage = "URL mal formada";
                await ShowErrorAlert("La URL de streaming no es válida.");
                return false;
            }

            // Validar esquema (http/https)
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                 StatusMessage = "Protocolo no soportado";
                 await ShowErrorAlert("Solo se soportan streams HTTP o HTTPS.");
                 return false;
            }

            Debug.WriteLine($"[RadioPlayer] Intentando reproducir: {station.Name} - {station.Url}");

            CurrentStation = station;
            IsBuffering = true;
            IsPlaying = false;
            StatusMessage = $"Validando stream...";

            // Pre-verificación profesional de la URL (HEAD request con timeout)
            try 
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                // Usamos GetAsync con HttpCompletionOption.ResponseHeadersRead para no descargar el stream completo
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode)
                {
                    StatusMessage = "Stream no disponible";
                    await ShowErrorAlert($"La emisora no responde (Código: {(int)response.StatusCode}).");
                    IsBuffering = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RadioPlayer] Pre-check fallido: {ex.Message}");
                StatusMessage = "Stream fuera de línea";
                await ShowErrorAlert("No se pudo conectar con el servidor de streaming. Reintente más tarde.");
                IsBuffering = false;
                return false;
            }

            StatusMessage = $"Conectando a {station.Name}...";

            if (_mediaElement != null)
            {
                // Ejecutar operaciones de MediaElement en el hilo principal para evitar crashes
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try 
                    {
                        // Detener reproducción anterior
                        _mediaElement.Stop();
                        
                        // Establecer nueva fuente
                        _mediaElement.Source = MediaSource.FromUri(uri);
                        
                        // Iniciar reproducción
                        _mediaElement.Play();
                        Debug.WriteLine($"[RadioPlayer] MediaElement.Play() invocado con éxito para {station.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[RadioPlayer] CRITICAL: Error accediendo a MediaElement: {ex.Message}");
                        throw; // Re-lanzar para manejarlo en el catch externo
                    }
                });
                
                return true;
            }
            else
            {
                StatusMessage = "Reproductor no inicializado";
                Debug.WriteLine("[RadioPlayer] MediaElement es NULL");
                await ShowErrorAlert("El componente de audio no está listo. Reinicia la aplicación.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] ERROR GLOBAL en PlayAsync: {ex.Message}");
            Debug.WriteLine(ex.StackTrace);
            
            StatusMessage = "Error al reproducir";
            IsPlaying = false;
            IsBuffering = false;
            
            await ShowErrorAlert($"No se pudo reproducir la emisora. Detalles: {ex.Message}");
            return false;
        }
    }

    public void Pause()
    {
        try
        {
            if (_mediaElement != null && IsPlaying)
            {
                _mediaElement.Pause();
                IsPlaying = false;
                StatusMessage = CurrentStation != null ? $"Pausado: {CurrentStation.Name}" : "Pausado";
                Debug.WriteLine("[RadioPlayer] Reproducción pausada");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] ERROR en Pause: {ex.Message}");
        }
    }

    public void Resume()
    {
        try
        {
            if (_mediaElement != null && !IsPlaying && CurrentStation != null)
            {
                _mediaElement.Play();
                IsPlaying = true;
                StatusMessage = $"Reproduciendo: {CurrentStation.Name}";
                Debug.WriteLine("[RadioPlayer] Reproducción reanudada");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] ERROR en Resume: {ex.Message}");
        }
    }

    public void TogglePlayback()
    {
        if (IsPlaying)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    public void Stop()
    {
        try
        {
            if (_mediaElement != null)
            {
                _mediaElement.Stop();
                IsPlaying = false;
                IsBuffering = false;
                StatusMessage = "Detenido";
                Debug.WriteLine("[RadioPlayer] Reproducción detenida");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] ERROR en Stop: {ex.Message}");
        }
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        Debug.WriteLine($"[RadioPlayer] MediaOpened - Stream conectado exitosamente");
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBuffering = false;
            IsPlaying = true;
            StatusMessage = CurrentStation != null ? $"Reproduciendo: {CurrentStation.Name}" : "Reproduciendo";
        });
    }

    private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        Debug.WriteLine($"[RadioPlayer] MediaFailed: {e.ErrorMessage}");
        
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            IsBuffering = false;
            IsPlaying = false;
            StatusMessage = "Error de reproducción";
            
            await ShowErrorAlert($"No se pudo conectar al stream: {e.ErrorMessage}");
        });
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (sender is not MediaElement mediaElement)
            return;
            
        Debug.WriteLine($"[RadioPlayer] Estado cambiado a: {mediaElement.CurrentState}");
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (mediaElement.CurrentState)
            {
                case MediaElementState.Opening:
                    IsBuffering = true;
                    StatusMessage = "Conectando...";
                    break;
                    
                case MediaElementState.Buffering:
                    IsBuffering = true;
                    StatusMessage = "Cargando...";
                    break;
                    
                case MediaElementState.Playing:
                    IsBuffering = false;
                    IsPlaying = true;
                    StatusMessage = CurrentStation != null ? $"Reproduciendo: {CurrentStation.Name}" : "Reproduciendo";
                    break;
                    
                case MediaElementState.Paused:
                    IsBuffering = false;
                    IsPlaying = false;
                    StatusMessage = "Pausado";
                    break;
                    
                case MediaElementState.Stopped:
                    IsBuffering = false;
                    IsPlaying = false;
                    StatusMessage = "Detenido";
                    break;
                    
                case MediaElementState.Failed:
                    IsBuffering = false;
                    IsPlaying = false;
                    StatusMessage = "Error";
                    break;
            }
        });
    }

    private async Task ShowErrorAlert(string message)
    {
        try
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error de Reproducción",
                    message,
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] No se pudo mostrar alerta: {ex.Message}");
        }
    }

    public void Cleanup()
    {
        try
        {
            Stop();
            
            if (_mediaElement != null)
            {
                _mediaElement.MediaOpened -= OnMediaOpened;
                _mediaElement.MediaFailed -= OnMediaFailed;
                _mediaElement.StateChanged -= OnStateChanged;
                _mediaElement = null;
            }
            
            CurrentStation = null;
            Debug.WriteLine("[RadioPlayer] Recursos liberados");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RadioPlayer] ERROR en Cleanup: {ex.Message}");
        }
    }
}
