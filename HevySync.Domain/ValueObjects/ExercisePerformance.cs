using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

/// <summary>
/// Represents the performance of an exercise during a workout.
/// Tracks the sets completed and whether the exercise was successful.
/// </summary>
public sealed class ExercisePerformance : ValueObject
{
    public Guid ExerciseId { get; }
    public List<Set> CompletedSets { get; }
    public PerformanceResult Result { get; }

    private ExercisePerformance(Guid exerciseId, List<Set> completedSets, PerformanceResult result)
    {
        ExerciseId = exerciseId;
        CompletedSets = completedSets;
        Result = result;
    }

    public static ExercisePerformance Create(Guid exerciseId, List<Set> completedSets, PerformanceResult result)
    {
        if (exerciseId == Guid.Empty)
        {
            throw new ArgumentException("Exercise ID cannot be empty", nameof(exerciseId));
        }

        if (completedSets == null || completedSets.Count == 0)
        {
            throw new ArgumentException("Completed sets cannot be empty", nameof(completedSets));
        }

        return new ExercisePerformance(exerciseId, completedSets, result);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ExerciseId;
        yield return Result;
        foreach (var set in CompletedSets)
        {
            yield return set;
        }
    }
}

/// <summary>
/// Represents the result of an exercise performance.
/// </summary>
public enum PerformanceResult
{
    /// <summary>
    /// Exercise was completed successfully and should progress.
    /// </summary>
    Success,

    /// <summary>
    /// Exercise was completed but did not meet progression criteria.
    /// Weight/reps should stay the same.
    /// </summary>
    Maintained,

    /// <summary>
    /// Exercise was not completed successfully.
    /// May need to deload or reduce weight.
    /// </summary>
    Failed
}

