using HevySync.Domain.Common;

namespace HevySync.Domain.Events;

public sealed record ActivityStartedEvent(
    Guid ActivityId,
    Guid UserId,
    Guid WorkoutId,
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public ActivityStartedEvent(Guid activityId, Guid userId, Guid workoutId)
        : this(activityId, userId, workoutId, DateTimeOffset.UtcNow)
    {
    }
}
