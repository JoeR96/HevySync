namespace HevySync.Endpoints.Average2Savage.Responses;

public record WeekOneSessionsDto
{
    public Dictionary<int, List<SessionExerciseDto>> Sessions { get; set; } = new();
}

public record SessionExerciseDto
{
    public string ExerciseTemplateId { get; set; } = string.Empty;
    public int RestSeconds { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<SessionSetDto> Sets { get; set; } = new();
}

public record SessionSetDto
{
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
}

