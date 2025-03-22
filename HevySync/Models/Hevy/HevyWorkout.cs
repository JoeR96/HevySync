using System.Text.Json.Serialization;

namespace HevySync.Models;

public record HevyWorkout
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }

    [JsonPropertyName("start_time")] public DateTimeOffset StartTime { get; set; }

    [JsonPropertyName("end_time")] public DateTimeOffset EndTime { get; set; }

    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("exercises")] public List<HevyExercise> Exercises { get; set; }
}