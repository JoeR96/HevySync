using HevySync.IntegrationTests.Fixtures;

namespace HevySync.IntegrationTests;

[CollectionDefinition("Integration Tests", DisableParallelization = true)]
public class TestCollectionDefinition : ICollectionFixture<WebHostFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

