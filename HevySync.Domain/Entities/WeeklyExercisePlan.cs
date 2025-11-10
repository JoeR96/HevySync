using HevySync.Domain.Common;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Entities;

/// <summary>
/// Represents the planned sets for a specific exercise in a specific week.
/// This stores the prescribed weights and reps for each set that should be performed.
/// </summary>
public sealed class WeeklyExercisePlan : AggregateRoot<Guid>
{
    private readonly List<Set> _plannedSets = null!;

    public Guid WorkoutId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int Week { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<Set> PlannedSets => _plannedSets.AsReadOnly();

    // EF Core constructor
    private WeeklyExercisePlan()
    {
    }

    private WeeklyExercisePlan(
        Guid id,
        Guid workoutId,
        Guid exerciseId,
        int week,
        DateTimeOffset createdAt,
        List<Set> plannedSets) : base(id)
    {
        WorkoutId = workoutId;
        ExerciseId = exerciseId;
        Week = week;
        CreatedAt = createdAt;
        _plannedSets = plannedSets;
    }

    public static WeeklyExercisePlan Create(
        Guid workoutId,
        Guid exerciseId,
        int week,
        List<Set> plannedSets)
    {
        if (workoutId == Guid.Empty)
            throw new ArgumentException("Workout ID cannot be empty", nameof(workoutId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty", nameof(exerciseId));

        if (week < 1)
            throw new ArgumentException("Week must be at least 1", nameof(week));

        if (plannedSets == null || plannedSets.Count == 0)
            throw new ArgumentException("Planned sets cannot be empty", nameof(plannedSets));

        return new WeeklyExercisePlan(
            Guid.NewGuid(),
            workoutId,
            exerciseId,
            week,
            DateTimeOffset.UtcNow,
            plannedSets);
    }

    /// <summary>
    /// Updates the planned sets for this week (e.g., after progression is applied)
    /// </summary>
    public void UpdatePlannedSets(List<Set> newPlannedSets)
    {
        if (newPlannedSets == null || newPlannedSets.Count == 0)
            throw new ArgumentException("Planned sets cannot be empty", nameof(newPlannedSets));

        _plannedSets.Clear();
        _plannedSets.AddRange(newPlannedSets);
    }
}
