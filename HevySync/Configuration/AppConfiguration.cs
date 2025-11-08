using HevySync.Configuration.Options;
using HevySync.Facades;
using HevySync.Handlers;
using HevySync.Infrastructure.Persistence;
using HevySync.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HevySync.Configuration;

public static class AppConfiguration
{
    public static IServiceCollection AddHevyApiClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExternalApiOptions>(
            configuration.GetSection(ExternalApiOptions.SectionName));

        services.AddHttpContextAccessor()
            .AddTransient<HevyApiAuthHandler>()
            .AddHttpClient<HevyApiService>((serviceProvider, client) =>
            {
                var apiOptions = serviceProvider.GetRequiredService<IOptions<ExternalApiOptions>>();
                client.BaseAddress = new Uri(apiOptions.Value.HevyApi);
            })
            .AddHttpMessageHandler<HevyApiAuthHandler>();


        return services;
    }

    public static IServiceCollection AddDomainServices(
        this IServiceCollection services)
    {
        return services.AddScoped<IA2SWorkoutFacade, A2SWorkoutFacade>()
            .AddScoped<HypertrophyService>()
            .AddScoped<RepsPerSetService>();
    }



    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HypertrophyOptions>(
            configuration.GetSection(HypertrophyOptions.SectionName));

        services.Configure<WorkoutOptions>(
            configuration.GetSection(WorkoutOptions.SectionName));

        return services;
    }

    public static async Task RunPendingMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HevySyncDbContext>();

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
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
            options.AddDefaultPolicy(b => b.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithExposedHeaders("*"));
        });
        return services;
    }
}