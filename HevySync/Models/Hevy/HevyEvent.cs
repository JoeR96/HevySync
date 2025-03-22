using System.Text.Json.Serialization;

namespace HevySync.Models;

public record HevyEvent
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("workout")] public HevyWorkout HevyWorkout { get; set; }
}