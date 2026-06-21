using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Data.Interfaces;

namespace ServiceCity.Data.Repositories;

public class ServiceCategoryRepository(AppDbContext db) : IServiceCategoryRepository
{
    public async Task<List<ServiceCategory>> GetAllAsync()
        => await db.ServiceCategories.OrderBy(c => c.SortOrder).ToListAsync();

    public async Task<ServiceCategory?> FindAsync(int id)
        => await db.ServiceCategories.FindAsync(id);
}
