using ServiceCity.Core.Entities;

namespace ServiceCity.Data.Interfaces;

public interface IUserRepository
{
    IQueryable<User> Query();
    Task<User?> FindAsync(int id);
    Task<User?> GetByPhoneAsync(string normalizedPhone);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> PhoneExistsAsync(string normalizedPhone);
    Task<List<User>> SearchAsync(string term);
    void Add(User user);
}
