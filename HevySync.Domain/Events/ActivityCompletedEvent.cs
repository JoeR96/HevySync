using HevySync.Domain.Common;

namespace HevySync.Domain.Events;

public sealed record ActivityCompletedEvent(
    Guid ActivityId,
    Guid UserId,
    Guid WorkoutId,
    DateTimeOffset OccurredOn) : IDomainEvent
{
    public ActivityCompletedEvent(Guid activityId, Guid userId, Guid workoutId)
        : this(activityId, userId, workoutId, DateTimeOffset.UtcNow)
    {
    }
}
