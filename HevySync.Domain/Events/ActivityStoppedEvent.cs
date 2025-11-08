using HevySync.Domain.Common;

namespace HevySync.Domain.Events;

public sealed record ActivityStoppedEvent(
    Guid ActivityId,
    Guid UserId,
    Guid WorkoutId,
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public ActivityStoppedEvent(Guid activityId, Guid userId, Guid workoutId)
        : this(activityId, userId, workoutId, DateTimeOffset.UtcNow)
    {
    }
}
