using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;

namespace HevySync.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IWorkoutRepository Workouts { get; }
    IRepository<Activity, Guid> Activities { get; }
    IWorkoutSessionRepository WorkoutSessions { get; }
    IWeeklyExercisePlanRepository WeeklyExercisePlans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
