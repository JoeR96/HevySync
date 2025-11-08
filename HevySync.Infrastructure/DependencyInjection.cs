using HevySync.Domain.DomainServices;
using HevySync.Domain.Repositories;
using HevySync.Infrastructure.DomainServices;
using HevySync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HevySync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HevySyncDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("HevySync.Infrastructure")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<HypertrophyOptions>(
            configuration.GetSection(HypertrophyOptions.SectionName));

        services.AddScoped<ILinearProgressionCalculator, LinearProgressionCalculator>();
        services.AddScoped<IRepsPerSetCalculator, RepsPerSetCalculator>();
        services.AddScoped<ISetGenerationService, SetGenerationService>();

        return services;
    }
}

