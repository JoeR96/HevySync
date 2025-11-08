using HevySync.Application.Common;

namespace HevySync.Application.Workouts.Commands.GenerateWeekOne;

public sealed record GenerateWeekOneCommand : ICommand<Dictionary<int, List<SessionExerciseDto>>>
{
    public Guid WorkoutId { get; init; }
}

public record SessionExerciseDto
{
    public string ExerciseTemplateId { get; init; } = string.Empty;
    public int RestSeconds { get; init; }
    public string Notes { get; init; } = string.Empty;
    public List<SessionSetDto> Sets { get; init; } = new();
}

public record SessionSetDto
{
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
}

