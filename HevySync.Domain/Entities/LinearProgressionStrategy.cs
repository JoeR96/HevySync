using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Entities;

/// <summary>
/// Linear progression strategy using percentage-based training.
/// Follows the Average 2 Savage Hypertrophy program structure.
/// </summary>
public sealed class LinearProgressionStrategy : ExerciseProgression
{
    public TrainingMax TrainingMax { get; private set; } = null!;
    public WeightProgression WeightProgression { get; private set; } = null!;
    public int AttemptsBeforeDeload { get; private set; }
    public bool IsPrimary { get; private set; }

    public override ProgramType ProgramType => ProgramType.LinearProgression;

    // EF Core constructor
    private LinearProgressionStrategy()
    {
    }

    private LinearProgressionStrategy(
        Guid id,
        Guid exerciseId,
        TrainingMax trainingMax,
        WeightProgression weightProgression,
        int attemptsBeforeDeload,
        bool isPrimary) : base(id, exerciseId)
    {
        TrainingMax = trainingMax;
        WeightProgression = weightProgression;
        AttemptsBeforeDeload = attemptsBeforeDeload;
        IsPrimary = isPrimary;
    }

    public static LinearProgressionStrategy Create(
        Guid exerciseId,
        TrainingMax trainingMax,
        WeightProgression weightProgression,
        int attemptsBeforeDeload,
        bool isPrimary)
    {
        if (attemptsBeforeDeload < 1)
        {
            throw new ArgumentException("Attempts before deload must be at least 1", nameof(attemptsBeforeDeload));
        }

        return new LinearProgressionStrategy(
            Guid.NewGuid(),
            exerciseId,
            trainingMax,
            weightProgression,
            attemptsBeforeDeload,
            isPrimary);
    }

    /// <summary>
    /// Applies progression based on performance result.
    /// Success: Increase training max by weight progression
    /// Maintained: Keep training max the same
    /// Failed: Decrease training max (deload)
    /// </summary>
    public void ApplyProgression(PerformanceResult result)
    {
        TrainingMax = result switch
        {
            PerformanceResult.Success => TrainingMax.Create(TrainingMax.Value + WeightProgression.Value),
            PerformanceResult.Maintained => TrainingMax, // No change
            PerformanceResult.Failed => TrainingMax.Create(TrainingMax.Value - WeightProgression.Value),
            _ => throw new ArgumentException($"Unknown performance result: {result}")
        };
    }
}

