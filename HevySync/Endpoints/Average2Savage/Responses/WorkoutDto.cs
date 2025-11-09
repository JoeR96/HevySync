namespace HevySync.Endpoints.Average2Savage.Responses;

public class WorkoutResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkoutActivityResponseDto WorkoutActivity { get; set; } = null!;
    public List<ExerciseResponseDto> Exercises { get; set; } = new();
}

public class WorkoutActivityResponseDto
{
    public Guid? Id { get; set; }
    public Guid? WorkoutId { get; set; }
    public int Week { get; set; }
    public int Day { get; set; }
    public int WorkoutsInWeek { get; set; }
    public string? Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
