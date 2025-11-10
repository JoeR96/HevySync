using HevySync.Domain.Entities;
using System.Linq.Expressions;

namespace HevySync.Domain.Repositories;

public interface IWeeklyExercisePlanRepository : IRepository<WeeklyExercisePlan, Guid>
{
    /// <summary>
    /// Gets all planned exercise sets for a specific workout and week
    /// </summary>
    Task<List<WeeklyExercisePlan>> GetPlansForWeekAsync(
        Guid workoutId,
        int week,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the planned sets for a specific exercise in a specific week
    /// </summary>
    Task<WeeklyExercisePlan?> GetPlanForExerciseAsync(
        Guid exerciseId,
        int week,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if plans already exist for a workout week
    /// </summary>
    Task<bool> PlansExistForWeekAsync(
        Guid workoutId,
        int week,
        CancellationToken cancellationToken = default);
}
