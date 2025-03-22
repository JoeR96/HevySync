using HevySync.Data;
using HevySync.Handlers;
using HevySync.Identity;
using HevySync.Services;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Configuration;

public static class AppConfiguration
{
    public static IServiceCollection AddHevyApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor()
            .AddTransient<HevyApiAuthHandler>()
            .AddHttpClient<HevyApiService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ExternalApiUrls:HevyApi"]!);
            })
            .AddHttpMessageHandler<HevyApiAuthHandler>();

        return services;
    }

    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddDbContext<HevySyncDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DatabaseConnectionString");
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HevySync"));
        });
    }


    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddEntityFrameworkStores<HevySyncDbContext>();

        return services;
    }

    public static async Task RunPendingMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HevySyncDbContext>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
                Console.WriteLine($"✅ Applied {pendingMigrations.Count()} pending migrations.");
            }
            else
            {
                Console.WriteLine("✅ No pending migrations.");
            }
        }
    }

    public static IServiceCollection AddCorsWithPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                b => b.WithOrigins("http://localhost:5173") // Vite's default port
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("*"));
        });
        return services;
    }
}