namespace HevySync.Endpoints;

record HevyApiKeyRequest
{
    public string HevyApiKey { get; set; } = default!;
}