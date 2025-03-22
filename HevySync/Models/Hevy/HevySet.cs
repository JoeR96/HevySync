using System.Text.Json.Serialization;

namespace HevySync.Models;

public record HevySet
{
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("weight_kg")] public double? WeightKg { get; set; }

    [JsonPropertyName("reps")] public int? Reps { get; set; }

    [JsonPropertyName("distance_meters")] public double? DistanceMeters { get; set; }

    [JsonPropertyName("duration_seconds")] public double? DurationSeconds { get; set; }

    [JsonPropertyName("rpe")] public double? RPE { get; set; }

    [JsonPropertyName("custom_metric")] public string CustomMetric { get; set; }
}