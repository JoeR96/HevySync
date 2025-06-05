namespace HevySync.IntegrationTests;

public class WebHostFixture : IAsyncLifetime
{
    private static CustomWebApplicationFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.StarContainerAsync();
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }

    internal HttpClient GetHttpClient()
    {
        var client = _factory.CreateClient();
        return client;
    }
}