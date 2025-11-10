using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HevySync.Infrastructure.Persistence.Repositories;

public class WorkoutSessionRepository : Repository<WorkoutSession, Guid>, IWorkoutSessionRepository
{
    public WorkoutSessionRepository(HevySyncDbContext context) : base(context)
    {
    }

    public async Task<List<WorkoutSession>> FindWithExercisesAsync(
        Expression<Func<WorkoutSession, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Need to use Include to eagerly load the private backing field
        return await DbSet
            .Where(predicate)
            .Include("_exercisePerformances")
            .ToListAsync(cancellationToken);
    }
}
