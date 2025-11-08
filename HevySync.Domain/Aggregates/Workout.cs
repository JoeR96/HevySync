using HevySync.Domain.Common;
using HevySync.Domain.Entities;
using HevySync.Domain.Events;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Aggregates;

/// <summary>
/// Workout aggregate root.
/// Represents a complete training program with exercises and progression tracking.
/// </summary>
public sealed class Workout : AggregateRoot<Guid>
{
    private readonly List<Exercise> _exercises = new();

    public WorkoutName Name { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public WorkoutActivity Activity { get; private set; } = null!;
    
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    // EF Core constructor
    private Workout()
    {
    }

    private Workout(
        Guid id,
        WorkoutName name,
        Guid userId,
        WorkoutActivity activity,
        List<Exercise> exercises) : base(id)
    {
        Name = name;
        UserId = userId;
        Activity = activity;
        _exercises = exercises;
    }

    public static Workout Create(
        WorkoutName name,
        Guid userId,
        int workoutsInWeek,
        List<Exercise> exercises)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidWorkoutException("User ID cannot be empty");
        }

        if (exercises == null || exercises.Count == 0)
        {
            throw new InvalidWorkoutException("A workout must contain at least one exercise");
        }

        var workoutId = Guid.NewGuid();
        var activity = WorkoutActivity.CreateInitial(workoutsInWeek);

        // Validate that all exercises belong to valid days
        var maxDay = exercises.Max(e => e.Day);
        if (maxDay > workoutsInWeek)
        {
            throw new InvalidWorkoutException($"Exercise day ({maxDay}) cannot exceed workouts in week ({workoutsInWeek})");
        }

        var workout = new Workout(
            workoutId,
            name,
            userId,
            activity,
            exercises);

        workout.RaiseDomainEvent(new WorkoutCreatedEvent(workoutId, name, userId, DateTimeOffset.UtcNow));

        return workout;
    }

    private void AdvanceToNextDay()
    {
        Activity = Activity.AdvanceDay();
    }

    public List<Exercise> GetExercisesForDay(int day)
    {
        return _exercises.Where(e => e.Day == day).OrderBy(e => e.Order).ToList();
    }

    /// <summary>
    /// Completes the current workout day with exercise performance data.
    /// Raises a WorkoutDayCompletedEvent and advances to the next day.
    /// </summary>
    public void CompleteDay(List<ExercisePerformance> exercisePerformances)
    {
        if (exercisePerformances == null || exercisePerformances.Count == 0)
        {
            throw new InvalidWorkoutException("Exercise performances cannot be empty");
        }

        // Validate that all exercises for the current day have performance data
        var exercisesForDay = GetExercisesForDay(Activity.Day);
        var performanceExerciseIds = exercisePerformances.Select(p => p.ExerciseId).ToHashSet();

        foreach (var exercise in exercisesForDay)
        {
            if (!performanceExerciseIds.Contains(exercise.Id))
            {
                throw new InvalidWorkoutException($"Missing performance data for exercise {exercise.Id}");
            }
        }

        // Raise domain event
        RaiseDomainEvent(new WorkoutDayCompletedEvent(
            Id,
            Activity.Week,
            Activity.Day,
            exercisePerformances,
            DateTimeOffset.UtcNow));

        // Advance to next day
        AdvanceToNextDay();
    }

    /// <summary>
    /// Applies progression to exercises based on their performance.
    /// Should be called after completing a week to prepare for the next week.
    /// </summary>
    public void ApplyProgression(List<ExercisePerformance> weekPerformances)
    {
        if (weekPerformances == null || weekPerformances.Count == 0)
        {
            throw new InvalidWorkoutException("Week performances cannot be empty");
        }

        // Group performances by exercise ID and determine overall performance
        var exercisePerformanceMap = weekPerformances
            .GroupBy(p => p.ExerciseId)
            .ToDictionary(
                g => g.Key,
                g => DetermineOverallPerformance(g.Select(p => p.Result).ToList()));

        // Apply progression to each exercise
        foreach (var exercise in _exercises)
        {
            if (exercisePerformanceMap.TryGetValue(exercise.Id, out var overallResult))
            {
                switch (exercise.Progression)
                {
                    case LinearProgressionStrategy lp:
                        lp.ApplyProgression(overallResult);
                        break;
                    case RepsPerSetStrategy rps:
                        rps.ApplyProgression(overallResult);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Determines the overall performance for an exercise based on multiple day performances.
    /// If any day failed, overall is Failed.
    /// If all days succeeded, overall is Success.
    /// Otherwise, overall is Maintained.
    /// </summary>
    private static PerformanceResult DetermineOverallPerformance(List<PerformanceResult> results)
    {
        if (results.Any(r => r == PerformanceResult.Failed))
        {
            return PerformanceResult.Failed;
        }

        if (results.All(r => r == PerformanceResult.Success))
        {
            return PerformanceResult.Success;
        }

        return PerformanceResult.Maintained;
    }
}

public class InvalidWorkoutException : DomainException
{
    public InvalidWorkoutException(string message) : base(message)
    {
    }
}

