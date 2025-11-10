using HevySync.Domain.Common;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Entities;

/// <summary>
/// Represents a completed workout session for a specific day.
/// Stores historical performance data including sets for each exercise.
/// </summary>
public sealed class WorkoutSession : AggregateRoot<Guid>
{
    private readonly List<SessionExercisePerformance> _exercisePerformances = new();

    public Guid WorkoutId { get; private set; }
    public int Week { get; private set; }
    public int Day { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    public IReadOnlyCollection<SessionExercisePerformance> ExercisePerformances => _exercisePerformances.AsReadOnly();

    // EF Core constructor
    private WorkoutSession()
    {
    }

    private WorkoutSession(
        Guid id,
        Guid workoutId,
        int week,
        int day,
        DateTimeOffset completedAt,
        List<SessionExercisePerformance> exercisePerformances) : base(id)
    {
        WorkoutId = workoutId;
        Week = week;
        Day = day;
        CompletedAt = completedAt;
        _exercisePerformances = exercisePerformances;
    }

    public static WorkoutSession Create(
        Guid workoutId,
        int week,
        int day,
        DateTimeOffset completedAt,
        List<ExercisePerformance> exercisePerformances)
    {
        if (workoutId == Guid.Empty)
            throw new ArgumentException("Workout ID cannot be empty", nameof(workoutId));

        if (week < 1)
            throw new ArgumentException("Week must be at least 1", nameof(week));

        if (day < 1)
            throw new ArgumentException("Day must be at least 1", nameof(day));

        if (exercisePerformances == null || exercisePerformances.Count == 0)
            throw new ArgumentException("Exercise performances cannot be empty", nameof(exercisePerformances));

        var sessionId = Guid.NewGuid();
        var sessionPerformances = exercisePerformances
            .Select(ep => SessionExercisePerformance.Create(
                sessionId,
                ep.ExerciseId,
                ep.CompletedSets,
                ep.Result))
            .ToList();

        return new WorkoutSession(
            sessionId,
            workoutId,
            week,
            day,
            completedAt,
            sessionPerformances);
    }
}

/// <summary>
/// Represents the performance of a single exercise within a workout session.
/// </summary>
public sealed class SessionExercisePerformance : Entity<Guid>
{
    private readonly List<Set> _completedSets = new();

    public Guid WorkoutSessionId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public PerformanceResult Result { get; private set; }

    public IReadOnlyCollection<Set> CompletedSets => _completedSets.AsReadOnly();

    // EF Core constructor
    private SessionExercisePerformance()
    {
    }

    private SessionExercisePerformance(
        Guid id,
        Guid workoutSessionId,
        Guid exerciseId,
        List<Set> completedSets,
        PerformanceResult result) : base(id)
    {
        WorkoutSessionId = workoutSessionId;
        ExerciseId = exerciseId;
        _completedSets = completedSets;
        Result = result;
    }

    public static SessionExercisePerformance Create(
        Guid workoutSessionId,
        Guid exerciseId,
        List<Set> completedSets,
        PerformanceResult result)
    {
        if (workoutSessionId == Guid.Empty)
            throw new ArgumentException("Workout session ID cannot be empty", nameof(workoutSessionId));

        if (exerciseId == Guid.Empty)
            throw new ArgumentException("Exercise ID cannot be empty", nameof(exerciseId));

        if (completedSets == null || completedSets.Count == 0)
            throw new ArgumentException("Completed sets cannot be empty", nameof(completedSets));

        return new SessionExercisePerformance(
            Guid.NewGuid(),
            workoutSessionId,
            exerciseId,
            completedSets,
            result);
    }
}
