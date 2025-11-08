using HevySync.Domain.Common;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Events;

public sealed record WorkoutCreatedEvent(
    Guid WorkoutId,
    WorkoutName WorkoutName,
    Guid UserId,
    DateTimeOffset OccurredOn) : IDomainEvent;

