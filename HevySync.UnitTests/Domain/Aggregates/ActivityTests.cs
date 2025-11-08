using FluentAssertions;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Enums;
using HevySync.Domain.Events;
using HevySync.Domain.ValueObjects;
using NUnit.Framework;

namespace HevySync.UnitTests.Domain.Aggregates;

[TestFixture]
public class ActivityTests
{
    [Test]
    public void Create_WithValidParameters_ShouldCreateActiveActivity()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var workoutName = WorkoutName.Create("Test Workout");

        var activity = Activity.Create(userId, workoutId, workoutName);

        activity.Should().NotBeNull();
        activity.UserId.Should().Be(userId);
        activity.WorkoutId.Should().Be(workoutId);
        activity.WorkoutName.Should().Be(workoutName);
        activity.Status.Should().Be(ActivityStatus.Active);
        activity.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        activity.CompletedAt.Should().BeNull();
        activity.StoppedAt.Should().BeNull();
    }

    [Test]
    public void Create_ShouldRaiseActivityStartedEvent()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var workoutName = WorkoutName.Create("Test Workout");

        var activity = Activity.Create(userId, workoutId, workoutName);

        activity.DomainEvents.Should().HaveCount(1);
        activity.DomainEvents.First().Should().BeOfType<ActivityStartedEvent>();
        var domainEvent = activity.DomainEvents.First() as ActivityStartedEvent;
        domainEvent!.UserId.Should().Be(userId);
        domainEvent.WorkoutId.Should().Be(workoutId);
    }

    [Test]
    public void Complete_WhenActive_ShouldMarkAsCompleted()
    {
        var activity = CreateTestActivity();

        activity.Complete();

        activity.Status.Should().Be(ActivityStatus.Completed);
        activity.CompletedAt.Should().NotBeNull();
        activity.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Complete_WhenActive_ShouldRaiseActivityCompletedEvent()
    {
        var activity = CreateTestActivity();
        activity.ClearDomainEvents();

        activity.Complete();

        activity.DomainEvents.Should().HaveCount(1);
        activity.DomainEvents.First().Should().BeOfType<ActivityCompletedEvent>();
    }

    [Test]
    public void Complete_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        var activity = CreateTestActivity();
        activity.Complete();

        var act = () => activity.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already completed*");
    }

    [Test]
    public void Complete_WhenStopped_ShouldThrowInvalidOperationException()
    {
        var activity = CreateTestActivity();
        activity.Stop();

        var act = () => activity.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*stopped*");
    }

    [Test]
    public void Stop_WhenActive_ShouldMarkAsStopped()
    {
        var activity = CreateTestActivity();

        activity.Stop();

        activity.Status.Should().Be(ActivityStatus.Stopped);
        activity.StoppedAt.Should().NotBeNull();
        activity.StoppedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Stop_WhenActive_ShouldRaiseActivityStoppedEvent()
    {
        var activity = CreateTestActivity();
        activity.ClearDomainEvents();

        activity.Stop();

        activity.DomainEvents.Should().HaveCount(1);
        activity.DomainEvents.First().Should().BeOfType<ActivityStoppedEvent>();
    }

    [Test]
    public void Stop_WhenAlreadyStopped_ShouldThrowInvalidOperationException()
    {
        var activity = CreateTestActivity();
        activity.Stop();

        var act = () => activity.Stop();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already stopped*");
    }

    [Test]
    public void Stop_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        var activity = CreateTestActivity();
        activity.Complete();

        var act = () => activity.Stop();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*completed*");
    }

    private static Activity CreateTestActivity()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var workoutName = WorkoutName.Create("Test Workout");
        return Activity.Create(userId, workoutId, workoutName);
    }
}
