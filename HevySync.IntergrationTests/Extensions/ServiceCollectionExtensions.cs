using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HevySync.IntergrationTests.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceDbContextWithPostgresTestContainer<TContext>(
        this IServiceCollection services, string connectionString, string migrationsAssembly) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null) services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(migrationsAssembly));
        });

        return services;
    }
}