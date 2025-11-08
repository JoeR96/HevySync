using HevySync.Domain.Common;
using HevySync.Domain.Events;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Aggregates;

/// <summary>
/// Represents a user's active workout routine.
/// Users can only have one active activity at a time.
/// </summary>
public sealed class Activity : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid WorkoutId { get; private set; }
    public WorkoutName WorkoutName { get; private set; } = null!;
    public ActivityStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }

    private Activity() { }

    private Activity(
        Guid id,
        Guid userId,
        Guid workoutId,
        WorkoutName workoutName,
        DateTime startedAt) : base(id)
    {
        UserId = userId;
        WorkoutId = workoutId;
        WorkoutName = workoutName;
        Status = ActivityStatus.Active;
        StartedAt = startedAt;
    }

    public static Activity Create(
        Guid userId,
        Guid workoutId,
        WorkoutName workoutName)
    {
        var activity = new Activity(
            Guid.NewGuid(),
            userId,
            workoutId,
            workoutName,
            DateTime.UtcNow);

        activity.RaiseDomainEvent(new ActivityStartedEvent(activity.Id, userId, workoutId));

        return activity;
    }

    public void Complete()
    {
        if (Status != ActivityStatus.Active)
            throw new ActivityException($"Cannot complete activity with status {Status}");

        Status = ActivityStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ActivityCompletedEvent(Id, UserId, WorkoutId));
    }

    public void Stop()
    {
        if (Status != ActivityStatus.Active)
            throw new ActivityException($"Cannot stop activity with status {Status}");

        Status = ActivityStatus.Stopped;
        StoppedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ActivityStoppedEvent(Id, UserId, WorkoutId));
    }
}

public enum ActivityStatus
{
    Active,
    Completed,
    Stopped
}

public class ActivityException : DomainException
{
    public ActivityException(string message) : base(message) { }
}
