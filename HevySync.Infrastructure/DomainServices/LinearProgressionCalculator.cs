using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace HevySync.Infrastructure.DomainServices;

public class LinearProgressionCalculator : ILinearProgressionCalculator
{
    private readonly HypertrophyOptions _options;

    public LinearProgressionCalculator(IOptions<HypertrophyOptions> options)
    {
        _options = options.Value;
    }

    public Task<List<Set>> CalculateSetsAsync(
        LinearProgressionStrategy strategy,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default)
    {
        var sets = new List<Set>();
        var config = strategy.IsPrimary
            ? _options.HypertrophyBlock.Primary
            : _options.HypertrophyBlock.Auxiliary;

        var weekIndex = activity.Week - 1; // Convert to 0-based index

        // Crea te working sets
        for (var i = 0; i < config.Sets; i++)
        {
            var weight = CalculateWorkingWeight(weekIndex, strategy.IsPrimary, strategy.TrainingMax.Value);
            var reps = config.RepsPerSet[weekIndex];

            sets.Add(Set.Create(weight, reps));
        }

        // Last set is AMRAP
        if (sets.Count > 0)
        {
            var lastSet = sets[^1];
            sets[^1] = Set.Create(lastSet.WeightKg, config.AmrapRepTarget[weekIndex]);
        }

        return Task.FromResult(sets);
    }

    private decimal CalculateWorkingWeight(int weekIndex, bool isPrimary, decimal trainingMax)
    {
        var config = isPrimary
            ? _options.HypertrophyBlock.Primary
            : _options.HypertrophyBlock.Auxiliary;

        var intensity = config.Intensity[weekIndex];
        var workingWeight = trainingMax * intensity;

        // Round to nearest plate increment
        return RoundToNearestPlate(workingWeight, _options.DefaultRoundingValue);
    }

    private static decimal RoundToNearestPlate(decimal weight, decimal plateIncrement)
    {
        return Math.Round(weight / plateIncrement) * plateIncrement;
    }
}

public class HypertrophyOptions
{
    public const string SectionName = "Hypertrophy";

    public decimal DefaultRoundingValue { get; set; }
    public HypertrophyBlockOptions HypertrophyBlock { get; set; } = new();
}

public class HypertrophyBlockOptions
{
    public LiftOptions Primary { get; set; } = new();
    public LiftOptions Auxiliary { get; set; } = new();
}

public class LiftOptions
{
    public int[] AmrapRepTarget { get; set; } = Array.Empty<int>();
    public int[] RepsPerSet { get; set; } = Array.Empty<int>();
    public decimal[] Intensity { get; set; } = Array.Empty<decimal>();
    public int Sets { get; set; }
}