using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Infrastructure.Persistence.Repositories;

public class WeeklyExercisePlanRepository : Repository<WeeklyExercisePlan, Guid>, IWeeklyExercisePlanRepository
{
    public WeeklyExercisePlanRepository(HevySyncDbContext context) : base(context)
    {
    }

    public async Task<List<WeeklyExercisePlan>> GetPlansForWeekAsync(
        Guid workoutId,
        int week,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.WorkoutId == workoutId && p.Week == week)
            .ToListAsync(cancellationToken);
    }

    public async Task<WeeklyExercisePlan?> GetPlanForExerciseAsync(
        Guid exerciseId,
        int week,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.ExerciseId == exerciseId && p.Week == week, cancellationToken);
    }

    public async Task<bool> PlansExistForWeekAsync(
        Guid workoutId,
        int week,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(p => p.WorkoutId == workoutId && p.Week == week, cancellationToken);
    }
}
