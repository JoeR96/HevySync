using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.DomainServices;

/// <summary>
/// Domain service for calculating reps-per-set progression sets.
/// </summary>
public interface IRepsPerSetCalculator
{
    /// <summary>
    /// Calculates sets for a reps-per-set exercise.
    /// </summary>
    Task<List<Set>> CalculateSetsAsync(
        RepsPerSetStrategy strategy,
        CancellationToken cancellationToken = default);
}

