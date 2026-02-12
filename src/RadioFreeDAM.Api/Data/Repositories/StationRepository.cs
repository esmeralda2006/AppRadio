using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data.Entities;

namespace RadioFreeDAM.Api.Data.Repositories;

public class StationRepository
{
    private readonly AppDbContext _db;

    public StationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RadioStationEntity>> GetAllAsync(int limit = 100)
    {
        return await _db.RadioStations.Take(limit).ToListAsync();
    }

    public async Task<List<RadioStationEntity>> SearchAsync(string name, int limit = 250)
    {
        return await _db.RadioStations
            .Where(s => s.Name.Contains(name))
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<RadioStationEntity>> GetByCountryAsync(string country, int limit = 250)
    {
        return await _db.RadioStations
            .Where(s => s.Country == country)
            .Take(limit)
            .ToListAsync();
    }
}
