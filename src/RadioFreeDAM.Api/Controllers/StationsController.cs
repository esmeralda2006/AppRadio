using Microsoft.AspNetCore.Mvc;
using RadioFreeDAM.Api.Data.Repositories;

namespace RadioFreeDAM.Api.Controllers;

[ApiController]
[Route("api/stations")]
public class StationsController : ControllerBase
{
    private readonly StationRepository _stationRepository;

    public StationsController(StationRepository stationRepository)
    {
        _stationRepository = stationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try 
        {
            var stations = await _stationRepository.GetAllAsync(100);
            return Ok(stations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[STATIONS-GET] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al obtener emisoras" });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        try 
        {
            if (string.IsNullOrWhiteSpace(name)) 
                return Ok(new List<object>());
                
            var stations = await _stationRepository.SearchAsync(name, 250);
            return Ok(stations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[STATIONS-SEARCH] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error en la búsqueda de emisoras" });
        }
    }

    [HttpGet("country/{country}")]
    public async Task<IActionResult> ByCountry(string country)
    {
        try 
        {
            var stations = await _stationRepository.GetByCountryAsync(country, 250);
            return Ok(stations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[STATIONS-COUNTRY] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al filtrar por país" });
        }
    }
}
