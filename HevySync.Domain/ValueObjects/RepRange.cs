using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class RepRange : ValueObject
{
    public int MinimumReps { get; }
    public int TargetReps { get; }
    public int MaximumReps { get; }

    private RepRange(int minimumReps, int targetReps, int maximumReps)
    {
        MinimumReps = minimumReps;
        TargetReps = targetReps;
        MaximumReps = maximumReps;
    }

    public static RepRange Create(int minimumReps, int targetReps, int maximumReps)
    {
        if (minimumReps < 1)
        {
            throw new ArgumentException("Minimum reps must be at least 1", nameof(minimumReps));
        }

        if (targetReps < minimumReps)
        {
            throw new ArgumentException("Target reps must be greater than or equal to minimum reps", nameof(targetReps));
        }

        if (maximumReps < targetReps)
        {
            throw new ArgumentException("Maximum reps must be greater than or equal to target reps", nameof(maximumReps));
        }

        return new RepRange(minimumReps, targetReps, maximumReps);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MinimumReps;
        yield return TargetReps;
        yield return MaximumReps;
    }

    public override string ToString() => $"{MinimumReps}-{TargetReps}-{MaximumReps} reps";
}

