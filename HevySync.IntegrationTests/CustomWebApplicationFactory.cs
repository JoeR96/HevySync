using System.Security.Claims;
using System.Text.Encodings.Web;
using HevySync.Data;
using HevySync.Identity;
using HevySync.IntegrationTests.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace HevySync.IntegrationTests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserHelper.UserId.ToString()),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "testuser@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

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

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HevySyncDbContext>();

                dbContext.Database.Migrate();

                dbContext.Users.Add(new ApplicationUser
                {
                    Id = UserHelper.UserId,
                    UserName = "TestUser",
                    Email = "testuser@example.com",
                    SecurityStamp = Guid.NewGuid().ToString()
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