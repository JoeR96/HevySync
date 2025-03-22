using HevySync.Data;
using HevySync.Handlers;
using HevySync.Identity;
using HevySync.Services;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Configuration;

public static class AppConfiguration
{
    public static void AddHevyApiClient(this IServiceCollection services, IConfiguration configuration) =>

        services.AddHttpContextAccessor()
            .AddTransient<HevyApiAuthHandler>()
            .AddHttpClient<HevyApiService>(client =>
                {
                    client.BaseAddress = new Uri(configuration["ExternalApiUrls:HevyApi"]!);
                })
            .AddHttpMessageHandler<HevyApiAuthHandler>();
    
    public static void AddDataServices(this IServiceCollection services, IConfiguration configuration) =>  
        services.AddDbContext<HevySyncDbContext>(
            options => options.UseInMemoryDatabase("HevySyncDb"));
    
    public static void AddIdentityServices(this IServiceCollection services) =>
        services.AddIdentityApiEndpoints<ApplicationUser>()
            .AddEntityFrameworkStores<HevySyncDbContext>();
    
}