using Microsoft.EntityFrameworkCore;
using RadioFreeDAM.Api.Data.Entities;

namespace RadioFreeDAM.Api.Data.Repositories;

public class UserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(UserEntity user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _db.Users.AnyAsync(u => u.Username == username);
    }
}
