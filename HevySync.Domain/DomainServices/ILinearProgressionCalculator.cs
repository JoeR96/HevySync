using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.DomainServices;

/// <summary>
/// Domain service for calculating linear progression sets.
/// </summary>
public interface ILinearProgressionCalculator
{
    /// <summary>
    /// Calculates sets for a linear progression exercise.
    /// </summary>
    Task<List<Set>> CalculateSetsAsync(
        LinearProgressionStrategy strategy,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default);
}

