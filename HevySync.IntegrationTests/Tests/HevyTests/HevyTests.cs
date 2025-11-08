using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Hevy;
using HevySync.Endpoints.Hevy.Requests;
using HevySync.IntegrationTests.Extensions;
using Shouldly;

namespace HevySync.IntegrationTests.Tests.HevyTests;

[Collection("Integration Tests")]
public class HevyTests(
    WebHostFixture webHostFixture)
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();

    [Fact]
    public async Task SetKey()
    {
        var hevyApiKeyRequest = new HevyApiKeyRequest
        {
            HevyApiKey = "test"
        };
        var endpoint = HevyEndpoint.SetKey.GetFullRoutePath();

        var response = await _client.PostAsync<string>(
            endpoint,
            hevyApiKeyRequest);

        response.ShouldContain("Hevy API Key has been set for");
    }

    [Fact]
    public async Task SetEmptyKey()
    {
        var hevyApiKeyRequest = new HevyApiKeyRequest
        {
            HevyApiKey = ""
        };
        var endpoint = HevyEndpoint.SetKey.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            hevyApiKeyRequest);

        validationErrorResponse!.Errors.Count.ShouldBe(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "HevyApiKey" &&
            e.Message == "The string cannot be empty.");
    }
}