using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data.Entities;

namespace RadioFreeDAM.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RadioStationEntity> RadioStations { get; set; }
    public DbSet<FavoriteEntity> Favorites { get; set; }
}

