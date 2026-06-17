using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceCity.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = "Host=localhost;Port=5432;Database=servicecity_dev;Username=postgres;Password=REDACTED";

        // Attempt to read connection string from appsettings.json
        try
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "../ServiceCity/appsettings.json");
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var connStrings) &&
                    connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    connectionString = defaultConn.GetString() ?? connectionString;
                }
            }
        }
        catch
        {
            // Use fallback connection string if config file cannot be read
        }

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
