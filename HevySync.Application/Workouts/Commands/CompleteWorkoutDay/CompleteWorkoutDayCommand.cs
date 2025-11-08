using HevySync.Application.Common;

namespace HevySync.Application.Workouts.Commands.CompleteWorkoutDay;

public sealed record CompleteWorkoutDayCommand : ICommand<CompleteWorkoutDayResult>
{
    public Guid WorkoutId { get; init; }
    public List<ExercisePerformanceDto> ExercisePerformances { get; init; } = new();
}

public record ExercisePerformanceDto
{
    public Guid ExerciseId { get; init; }
    public List<CompletedSetDto> CompletedSets { get; init; } = new();
    public string PerformanceResult { get; init; } = string.Empty; // "Success", "Maintained", "Failed"
}

public record CompletedSetDto
{
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
}

public record CompleteWorkoutDayResult
{
    public Guid WorkoutId { get; init; }
    public int CompletedWeek { get; init; }
    public int CompletedDay { get; init; }
    public int NewWeek { get; init; }
    public int NewDay { get; init; }
    public bool WeekCompleted { get; init; }
}

