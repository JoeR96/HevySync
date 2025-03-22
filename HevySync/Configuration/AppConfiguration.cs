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
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("HeavySync"));
        });
    }


    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddEntityFrameworkStores<HevySyncDbContext>();

        return services;
    }
}