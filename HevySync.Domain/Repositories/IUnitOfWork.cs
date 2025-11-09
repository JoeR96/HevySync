using HevySync.Domain.Aggregates;

namespace HevySync.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IWorkoutRepository Workouts { get; }
    IRepository<Activity, Guid> Activities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
