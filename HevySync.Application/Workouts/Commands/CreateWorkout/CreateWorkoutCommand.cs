using HevySync.Application.Common;
using HevySync.Application.DTOs;

namespace HevySync.Application.Workouts.Commands.CreateWorkout;

public sealed record CreateWorkoutCommand : ICommand<WorkoutDto>
{
    public string WorkoutName { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public int WorkoutDaysInWeek { get; init; }
    public List<CreateExerciseDto> Exercises { get; init; } = new();
}

public record CreateExerciseDto
{
    public string ExerciseName { get; init; } = string.Empty;
    public string ExerciseTemplateId { get; init; } = string.Empty;
    public int RestTimer { get; init; }
    public int Day { get; init; }
    public int Order { get; init; }
    public int NumberOfSets { get; init; }
    public string? BodyCategory { get; init; }
    public string? EquipmentType { get; init; }
    public CreateProgressionDto Progression { get; init; } = null!;
}

public abstract record CreateProgressionDto
{
    public string ProgramType { get; init; } = string.Empty;
}

public record CreateLinearProgressionDto : CreateProgressionDto
{
    public decimal TrainingMax { get; init; }
    public decimal WeightProgression { get; init; }
    public int AttemptsBeforeDeload { get; init; }
    public bool IsPrimary { get; init; }
}

public record CreateRepsPerSetDto : CreateProgressionDto
{
    public int MinimumReps { get; init; }
    public int TargetReps { get; init; }
    public int MaximumReps { get; init; }
    public int StartingSetCount { get; init; }
    public int TargetSetCount { get; init; }
    public decimal StartingWeight { get; init; }
}

