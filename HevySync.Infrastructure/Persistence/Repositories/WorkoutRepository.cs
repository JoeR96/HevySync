using HevySync.Domain.Aggregates;
using HevySync.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Infrastructure.Persistence.Repositories;

public class WorkoutRepository(HevySyncDbContext context) : Repository<Workout, Guid>(context), IWorkoutRepository
{
    public override async Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include("_exercises.Progression")
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<Workout>> GetUserWorkoutsWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include("_exercises.Progression")
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
