using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HevySync.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating HevySyncDbContext instances.
/// This is used by EF Core tools (migrations, etc.) at design time.
/// </summary>
public class HevySyncDbContextFactory : IDesignTimeDbContextFactory<HevySyncDbContext>
{
    public HevySyncDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HevySyncDbContext>();
        
        // Use a dummy connection string for design-time operations
        // The actual connection string will be provided at runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=hevysync_design;Username=postgres;Password=postgres");
        
        return new HevySyncDbContext(optionsBuilder.Options);
    }
}

