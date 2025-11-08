using HevySync.Infrastructure.Identity;
using HevySync.Infrastructure.Persistence;
using HevySync.IntegrationTests.Auth;
using HevySync.IntegrationTests.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace HevySync.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private PostgreSqlContainer _postgresContainer = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.ReplaceDbContextWithPostgresTestContainer<HevySyncDbContext>(
                _postgresContainer.GetConnectionString(), "HevySync.Infrastructure");

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HevySyncDbContext>();

            dbContext.Database.Migrate();

            // Only add the test user if it doesn't already exist
            if (!dbContext.Users.Any(u => u.Id == UserHelper.UserId))
            {
                dbContext.Users.Add(new ApplicationUser
                {
                    Id = UserHelper.UserId,
                    UserName = "TestUser",
                    Email = "testuser@example.com",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    HevyApiKey = Guid.NewGuid().ToString()
                });
                dbContext.SaveChanges();
            }
        });
    }

    public async Task StarContainerAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("hevysync")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
    }
}