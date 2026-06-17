using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;

namespace ServiceCity.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceCategory>().HasData(
            new ServiceCategory { Id = 1, Name = "Repair", Description = "AC repair and troubleshooting", SortOrder = 1 },
            new ServiceCategory { Id = 2, Name = "Maintenance", Description = "Regular AC maintenance and cleaning", SortOrder = 2 },
            new ServiceCategory { Id = 3, Name = "Installation", Description = "New AC unit installation", SortOrder = 3 },
            new ServiceCategory { Id = 4, Name = "Gas Refill", Description = "Refrigerant gas refill service", SortOrder = 4 }
        );
    }
}
