using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Infrastructure.DomainServices;

public class RepsPerSetCalculator : IRepsPerSetCalculator
{
    public Task<List<Set>> CalculateSetsAsync(
        RepsPerSetStrategy strategy,
        CancellationToken cancellationToken = default)
    {
        var sets = new List<Set>();

        // Start with minimum reps for all starting sets
        for (var i = 0; i < strategy.StartingSetCount; i++)
        {
            sets.Add(Set.Create(strategy.StartingWeight, strategy.RepRange.MinimumReps));
        }

        return Task.FromResult(sets);
    }
}

