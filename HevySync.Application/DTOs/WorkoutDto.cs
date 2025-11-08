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
    public string? BodyCategory { get; init; }
    public string? EquipmentType { get; init; }
    public ExerciseProgressionDto Progression { get; init; } = null!;
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
}

