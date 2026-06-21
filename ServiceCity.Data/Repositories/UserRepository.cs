using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Data.Interfaces;

namespace ServiceCity.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public IQueryable<User> Query() => db.Users.AsNoTracking();

    public async Task<User?> FindAsync(int id)
        => await db.Users.FindAsync(id);

    public async Task<User?> GetByPhoneAsync(string normalizedPhone)
        => await db.Users.FirstOrDefaultAsync(u => u.PhoneNumberNormalized == normalizedPhone);

    public async Task<User?> GetByUsernameAsync(string username)
        => await db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> PhoneExistsAsync(string normalizedPhone)
        => await db.Users.AnyAsync(u => u.PhoneNumberNormalized == normalizedPhone);

    public async Task<List<User>> SearchAsync(string term)
        => await db.Users
            .Where(u => !u.IsAdmin)
            .Where(u =>
                u.Name.Contains(term) ||
                u.PhoneNumber.Contains(term) ||
                (u.PhoneNumberNormalized != null && u.PhoneNumberNormalized.Contains(term)))
            .ToListAsync();

    public void Add(User user) => db.Users.Add(user);
}
