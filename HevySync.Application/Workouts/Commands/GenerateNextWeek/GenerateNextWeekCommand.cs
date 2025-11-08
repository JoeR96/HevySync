using HevySync.Application.Common;
using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Domain.ValueObjects;

namespace HevySync.Application.Workouts.Commands.GenerateNextWeek;

/// <summary>
/// Command to generate the next week of a workout program.
/// Applies progression based on the previous week's performance.
/// </summary>
public sealed record GenerateNextWeekCommand : ICommand<Dictionary<int, List<SessionExerciseDto>>>
{
    public Guid WorkoutId { get; init; }
    public List<ExercisePerformanceDto> WeekPerformances { get; init; } = new();
}

/// <summary>
/// DTO representing exercise performance for the week.
/// </summary>
public record ExercisePerformanceDto
{
    public Guid ExerciseId { get; init; }
    public List<CompletedSetDto> CompletedSets { get; init; } = new();
    public string PerformanceResult { get; init; } = string.Empty; // "Success", "Maintained", "Failed"
}

/// <summary>
/// DTO representing a completed set.
/// </summary>
public record CompletedSetDto
{
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
}

