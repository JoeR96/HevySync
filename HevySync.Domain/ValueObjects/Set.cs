using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

/// <summary>
/// Represents a single set of an exercise with weight and reps.
/// </summary>
public sealed class Set : ValueObject
{
    public decimal WeightKg { get; private set; }
    public int Reps { get; private set; }

    private Set(decimal weightKg, int reps)
    {
        if (weightKg < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(weightKg));
        if (reps <= 0)
            throw new ArgumentException("Reps must be greater than zero", nameof(reps));

        WeightKg = weightKg;
        Reps = reps;
    }

    private Set()
    {
        // EF Core
    }

    public static Set Create(decimal weightKg, int reps)
    {
        return new Set(weightKg, reps);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return WeightKg;
        yield return Reps;
    }
}

