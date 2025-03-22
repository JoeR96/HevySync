using HevySync.Data;
using HevySync.IntergrationTests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;

namespace HevySync.IntergrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private PostgreSqlContainer _postgresContainer = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.ReplaceDbContextWithPostgresTestContainer<HevySyncDbContext>(
                _postgresContainer.GetConnectionString(), "HevySyncDb");
        });
    }

    public async Task StarContainerAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("scale-slayer")
            .WithUsername("postgres")
            .WithPassword("password")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
    }
}