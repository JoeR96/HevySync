using HevySync.Data;
using HevySync.Identity;
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
                _postgresContainer.GetConnectionString(), "HevySync");

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<HevySyncDbContext>();

            dbContext.Database.Migrate();

            dbContext.Users.Add(new ApplicationUser
            {
                Id = UserHelper.UserId,
                UserName = "TestUser",
                Email = "testuser@example.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                HevyApiKey = "bbc441b6-532e-4574-91c4-41b227f9f044"
            });
            dbContext.SaveChanges();
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