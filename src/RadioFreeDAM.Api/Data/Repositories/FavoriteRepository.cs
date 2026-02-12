using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data.Entities;

namespace RadioFreeDAM.Api.Data.Repositories;

public class FavoriteRepository
{
    private readonly AppDbContext _db;

    public FavoriteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FavoriteEntity>> GetByUserIdAsync(int userId)
    {
        return await _db.Favorites.Where(f => f.UserId == userId).ToListAsync();
    }

    public async Task AddAsync(FavoriteEntity favorite)
    {
        _db.Favorites.Add(favorite);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var fav = await _db.Favorites.FindAsync(id);
        if (fav != null)
        {
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteByCompositeKeyAsync(int userId, string stationId)
    {
        var fav = await _db.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.StationId == stationId);
        if (fav != null)
        {
            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int userId, string stationId)
    {
        return await _db.Favorites.AnyAsync(f => f.UserId == userId && f.StationId == stationId);
    }
}
