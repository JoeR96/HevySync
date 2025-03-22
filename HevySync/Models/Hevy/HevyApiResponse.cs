using System.Text.Json.Serialization;

namespace HevySync.Models;

public record HevyApiResponse
{
    [JsonPropertyName("page")] public int Page { get; set; }

    [JsonPropertyName("page_count")] public int PageCount { get; set; }

    [JsonPropertyName("events")] public List<HevyEvent> Events { get; set; }
}