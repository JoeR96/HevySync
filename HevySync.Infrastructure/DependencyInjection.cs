using HevySync.Domain.DomainServices;
using HevySync.Domain.Repositories;
using HevySync.Infrastructure.DomainServices;
using HevySync.Infrastructure.Persistence;
using HevySync.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HevySync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<HevySyncDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("HevySync.Infrastructure")));

        // Register repositories
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        // Configure options
        services.Configure<HypertrophyOptions>(
            configuration.GetSection(HypertrophyOptions.SectionName));

        // Register domain services
        services.AddScoped<ILinearProgressionCalculator, LinearProgressionCalculator>();
        services.AddScoped<IRepsPerSetCalculator, RepsPerSetCalculator>();
        services.AddScoped<ISetGenerationService, SetGenerationService>();

        return services;
    }
}

