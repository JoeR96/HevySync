using HevySync.Domain.Aggregates;

namespace HevySync.Domain.Repositories;

/// <summary>
/// Repository interface for Workout aggregate.
/// Defines the contract for persistence operations.
/// </summary>
public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Workout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Workout workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

