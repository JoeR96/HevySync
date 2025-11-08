using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.DomainServices;

/// <summary>
/// Domain service for generating sets based on progression strategy.
/// </summary>
public interface ISetGenerationService
{
    /// <summary>
    /// Generates sets for week one of a program.
    /// </summary>
    Task<List<Set>> GenerateWeekOneSetsAsync(
        ExerciseProgression progression,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default);
}

