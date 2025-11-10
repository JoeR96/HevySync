using HevySync.Domain.Entities;
using System.Linq.Expressions;

namespace HevySync.Domain.Repositories;

public interface IWorkoutSessionRepository : IRepository<WorkoutSession, Guid>
{
    Task<List<WorkoutSession>> FindWithExercisesAsync(
        Expression<Func<WorkoutSession, bool>> predicate,
        CancellationToken cancellationToken = default);
}
