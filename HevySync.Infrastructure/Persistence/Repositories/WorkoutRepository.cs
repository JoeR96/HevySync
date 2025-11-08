using HevySync.Domain.Aggregates;
using HevySync.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Infrastructure.Persistence.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly HevySyncDbContext _dbContext;

    public WorkoutRepository(HevySyncDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .Include("_exercises.Progression")
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<Workout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Workouts
            .Where(w => w.UserId == userId)
            .Include("_exercises.Progression")
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        await _dbContext.Workouts.AddAsync(workout, cancellationToken);
    }

    public Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        _dbContext.Workouts.Update(workout);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        _dbContext.Workouts.Remove(workout);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

