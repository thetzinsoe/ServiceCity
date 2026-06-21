using ServiceCity.Core.Entities;

namespace ServiceCity.Data.Interfaces;

public interface IServiceCategoryRepository
{
    Task<List<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory?> FindAsync(int id);
}
