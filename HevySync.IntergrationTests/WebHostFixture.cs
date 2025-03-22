namespace HevySync.IntergrationTests;

[SetUpFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class WebHostFixture
{
    private static CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.StarContainerAsync();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _factory.Dispose();
    }

    internal static HttpClient GetHttpClient()
    {
        return _factory.CreateClient();
    }
}