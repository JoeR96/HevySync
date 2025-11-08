using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Entities;

/// <summary>
/// Reps per set progression strategy.
/// Focuses on increasing volume through set progression.
/// </summary>
public sealed class RepsPerSetStrategy : ExerciseProgression
{
    public RepRange RepRange { get; private set; } = null!;
    public int StartingSetCount { get; private set; }
    public int TargetSetCount { get; private set; }
    public decimal StartingWeight { get; private set; }
    public WeightProgression WeightProgression { get; private set; } = null!;

    public override ProgramType ProgramType => ProgramType.RepsPerSet;

    // EF Core constructor
    private RepsPerSetStrategy()
    {
    }

    private RepsPerSetStrategy(
        Guid id,
        Guid exerciseId,
        RepRange repRange,
        int startingSetCount,
        int targetSetCount,
        decimal startingWeight,
        WeightProgression weightProgression) : base(id, exerciseId)
    {
        RepRange = repRange;
        StartingSetCount = startingSetCount;
        TargetSetCount = targetSetCount;
        StartingWeight = startingWeight;
        WeightProgression = weightProgression;
    }

    public static RepsPerSetStrategy Create(
        Guid exerciseId,
        RepRange repRange,
        int startingSetCount,
        int targetSetCount,
        decimal startingWeight,
        WeightProgression weightProgression)
    {
        if (startingSetCount < 1)
            throw new ArgumentException("Starting set count must be at least 1", nameof(startingSetCount));

        if (targetSetCount < startingSetCount)
            throw new ArgumentException("Target set count must be greater than or equal to starting set count", nameof(targetSetCount));

        if (startingWeight < 0)
            throw new ArgumentException("Starting weight cannot be negative", nameof(startingWeight));

        return new RepsPerSetStrategy(
            Guid.NewGuid(),
            exerciseId,
            repRange,
            startingSetCount,
            targetSetCount,
            startingWeight,
            weightProgression);
    }

    /// <summary>
    /// Applies progression based on performance result.
    /// Success: Increase set count (if not at target), otherwise increase weight
    /// Maintained: Keep current values
    /// Failed: Decrease set count or weight
    /// </summary>
    public void ApplyProgression(PerformanceResult result)
    {
        switch (result)
        {
            case PerformanceResult.Success:
                if (StartingSetCount < TargetSetCount)
                    StartingSetCount++;
                else
                    StartingWeight += WeightProgression.Value;
                break;

            case PerformanceResult.Maintained:
                break;

            case PerformanceResult.Failed:
                if (StartingSetCount > 1)
                    StartingSetCount--;
                else if (StartingWeight >= WeightProgression.Value)
                    StartingWeight -= WeightProgression.Value;
                break;

            default:
                throw new ArgumentException($"Unknown performance result: {result}");
        }
    }
}

