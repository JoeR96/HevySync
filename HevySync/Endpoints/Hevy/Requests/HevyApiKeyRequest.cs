namespace HevySync.Endpoints.Hevy.Requests;

internal record HevyApiKeyRequest
{
    public string HevyApiKey { get; set; } = default!;
}