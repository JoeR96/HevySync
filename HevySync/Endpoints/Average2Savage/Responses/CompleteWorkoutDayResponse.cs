namespace HevySync.Endpoints.Average2Savage.Responses;

public sealed record CompleteWorkoutDayResponse
{
    public Guid WorkoutId { get; init; }
    public int CompletedWeek { get; init; }
    public int CompletedDay { get; init; }
    public int NewWeek { get; init; }
    public int NewDay { get; init; }
    public bool WeekCompleted { get; init; }
}

