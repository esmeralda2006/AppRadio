using System.Net.Http.Json;
using RadioFreeDAM.Api.Data.Entities;

namespace RadioFreeDAM.Api.Services;

public class RadioBrowserService
{
    private readonly HttpClient _http;

    public RadioBrowserService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<RadioStationEntity>> GetStationsAsync()
    {
        var url = "https://de1.api.radio-browser.info/json/stations/topclick/50";

        // RadioBrowser API requires a User-Agent
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _http.DefaultRequestHeaders.Add("User-Agent", "RadioFreeDAM/1.0");
        }

        var radios = await _http.GetFromJsonAsync<List<RadioBrowserDto>>(url);

        return radios.Select(r => new RadioStationEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = r.name,
            Url = r.url_resolved,
            ImageUrl = r.favicon,
            Country = r.country,
            Genre = r.tags,
            Tags = r.tags
        }).ToList();
    }
}

public class RadioBrowserDto
{
    public string name { get; set; }
    public string url_resolved { get; set; }
    public string favicon { get; set; }
    public string country { get; set; }
    public string tags { get; set; }
}
