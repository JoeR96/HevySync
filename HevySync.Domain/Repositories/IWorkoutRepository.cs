using HevySync.Domain.Aggregates;

namespace HevySync.Domain.Repositories;

public interface IWorkoutRepository : IRepository<Workout, Guid>
{
    Task<List<Workout>> GetUserWorkoutsWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
}
