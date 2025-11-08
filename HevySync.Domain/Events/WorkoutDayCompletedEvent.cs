using HevySync.Domain.Common;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Events;

/// <summary>
/// Domain event raised when a workout day is completed with performance data.
/// </summary>
public sealed record WorkoutDayCompletedEvent(
    Guid WorkoutId,
    int Week,
    int Day,
    List<ExercisePerformance> ExercisePerformances,
    DateTimeOffset OccurredOn) : IDomainEvent;

