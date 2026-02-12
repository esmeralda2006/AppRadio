using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data;
using RadioFreeDAM.Api.Services;

namespace RadioFreeDAM.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly RadioBrowserService _service;
    private readonly AppDbContext _db;

    public SyncController(
        RadioBrowserService service,
        AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost("radiobrowser")]
    public async Task<IActionResult> SyncRadios()
    {
        try
        {
            var radios = await _service.GetStationsAsync();
            int addedCount = 0;

            // Evitar duplicados consultando URLs existentes
            var existingUrls = await _db.RadioStations.Select(x => x.Url).ToListAsync();
            var existingUrlSet = new HashSet<string>(existingUrls);

            foreach (var r in radios)
            {
                if (!existingUrlSet.Contains(r.Url))
                {
                    _db.RadioStations.Add(r);
                    existingUrlSet.Add(r.Url);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await _db.SaveChangesAsync();
            }

            return Ok(new { count = addedCount, totalSynced = radios.Count });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SYNC] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error sincronizando emisoras" });
        }
    }
}
