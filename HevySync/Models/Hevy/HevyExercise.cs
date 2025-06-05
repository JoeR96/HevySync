using System.Text.Json.Serialization;

namespace HevySync.Models.Hevy;

public record HevyExercise
{
    [JsonPropertyName("index")] public int Index { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("notes")] public string Notes { get; set; }

    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; }

    [JsonPropertyName("superset_id")] public string SupersetId { get; set; }

    [JsonPropertyName("sets")] public List<HevySet> Sets { get; set; }
}