namespace HevySync.IntegrationTests.Fixtures;

public class WebHostFixture : IAsyncLifetime
{
    private static readonly object _lock = new object();
    private static CustomWebApplicationFactory? _factory;
    private static bool _initialized = false;

    public async Task InitializeAsync()
    {
        lock (_lock)
        {
            if (!_initialized)
            {
                _factory = new CustomWebApplicationFactory();
                _factory.StarContainerAsync().GetAwaiter().GetResult();
                _initialized = true;
            }
        }
        await Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    internal HttpClient GetHttpClient()
    {
        if (_factory == null)
            throw new InvalidOperationException("Factory not initialized");

        var client = _factory.CreateClient();
        return client;
    }
}