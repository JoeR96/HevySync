namespace HevySync.Endpoints.Average2Savage.Responses;

public record ExerciseDto
{
    public Guid Id { get; set; }
    public string ExerciseName { get; set; }
    public int Day { get; set; }
    public int RestTimer { get; set; }
    public ExerciseDetailDto ExerciseDetail { get; set; }
    public int Order { get; set; }
    public int NumberOfSets { get; set; }
}