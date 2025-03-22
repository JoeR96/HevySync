namespace HevySync.Endpoints.Requests;

internal record HevyApiKeyRequest
{
    public string HevyApiKey { get; set; } = default!;
}