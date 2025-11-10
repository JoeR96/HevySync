namespace HevySync.Application.DTOs;

public record WorkoutDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public WorkoutActivityDto Activity { get; init; } = null!;
    public List<ExerciseDto> Exercises { get; init; } = new();
}

public record WorkoutActivityDto
{
    public int Week { get; init; }
    public int Day { get; init; }
    public int WorkoutsInWeek { get; init; }
    public string? Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record ExerciseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ExerciseTemplateId { get; init; } = string.Empty;
    public int RestTimer { get; init; }
    public int Day { get; init; }
    public int Order { get; init; }
    public int NumberOfSets { get; init; }
    public ExerciseProgressionDto Progression { get; init; } = null!;
    public List<SetDto>? PlannedSets { get; init; }
}

public abstract record ExerciseProgressionDto
{
    public Guid Id { get; init; }
    public string ProgramType { get; init; } = string.Empty;
}

public record LinearProgressionDto : ExerciseProgressionDto
{
    public decimal TrainingMax { get; init; }
    public decimal WeightProgression { get; init; }
    public int AttemptsBeforeDeload { get; init; }
    public bool IsPrimary { get; init; }
}

public record RepsPerSetDto : ExerciseProgressionDto
{
    public int MinimumReps { get; init; }
    public int TargetReps { get; init; }
    public int MaximumReps { get; init; }
    public int StartingSetCount { get; init; }
    public int TargetSetCount { get; init; }
    public decimal StartingWeight { get; init; }
    public decimal WeightProgression { get; init; }
}

public record WorkoutSessionDto
{
    public Guid Id { get; init; }
    public Guid WorkoutId { get; init; }
    public int Week { get; init; }
    public int Day { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
    public List<SessionExercisePerformanceDto> ExercisePerformances { get; init; } = new();
}

public record SessionExercisePerformanceDto
{
    public Guid Id { get; init; }
    public Guid ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public string ExerciseTemplateId { get; init; } = string.Empty;
    public string PerformanceResult { get; init; } = string.Empty;
    public List<SetDto> CompletedSets { get; init; } = new();
}

public record SetDto
{
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
}

/// <summary>
/// DTO for the Workout Planning Context - represents a planned exercise with its sets
/// </summary>
public record PlannedExerciseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ExerciseTemplateId { get; init; } = string.Empty;
    public int RestTimer { get; init; }
    public int Day { get; init; }
    public int Order { get; init; }
    public int NumberOfSets { get; init; }
    public ExerciseProgressionDto Progression { get; init; } = null!;
    public List<SetDto> PlannedSets { get; init; } = new();
    public bool IsCompleted { get; init; }
}

/// <summary>
/// DTO for the Workout Planning Context - represents the current week's planned exercises
/// </summary>
public record CurrentWeekPlannedExercisesDto
{
    public Guid WorkoutId { get; init; }
    public string WorkoutName { get; init; } = string.Empty;
    public int Week { get; init; }
    public int CurrentDay { get; init; }
    public int TotalDaysInWeek { get; init; }
    public Dictionary<int, List<PlannedExerciseDto>> ExercisesByDay { get; init; } = new();
}
