using System.Text.Json.Serialization;

namespace HevySync.Models;

public class HevyEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("workout")]
    public HevyWorkout HevyWorkout { get; set; }
}