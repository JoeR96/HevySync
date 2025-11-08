using FluentAssertions;
using HevySync.Application.Workouts.Commands.CompleteWorkoutDay;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;
using Moq;
using NUnit.Framework;

namespace HevySync.UnitTests.Application.Commands;

[TestFixture]
public class CompleteWorkoutDayCommandHandlerTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private CompleteWorkoutDayCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CompleteWorkoutDayCommandHandler(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldCompleteDay()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var command = new CompleteWorkoutDayCommand(
            workout.Id,
            new List<ExercisePerformanceDto>
            {
                new ExercisePerformanceDto(
                    exercise.Id,
                    new List<CompletedSetDto>
                    {
                        new CompletedSetDto(100m, 5),
                        new CompletedSetDto(100m, 5),
                        new CompletedSetDto(100m, 5)
                    },
                    PerformanceResult.Success)
            });

        _unitOfWorkMock.Setup(x => x.Workouts.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CompletedDay.Should().Be(1);
        result.NewDay.Should().Be(2);
        result.WeekCompleted.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithNonExistentWorkout_ShouldThrowInvalidOperationException()
    {
        var command = new CompleteWorkoutDayCommand(
            Guid.NewGuid(),
            new List<ExercisePerformanceDto>());

        _unitOfWorkMock.Setup(x => x.Workouts.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workout?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Test]
    public async Task Handle_OnLastDayOfWeek_ShouldIndicateWeekCompleted()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();

        for (int i = 0; i < 4; i++)
        {
            workout.CompleteDay(new List<ExercisePerformance>
            {
                ExercisePerformance.Create(
                    exercise.Id,
                    new List<Set> { Set.Create(100m, 5) },
                    PerformanceResult.Success)
            });
        }

        var command = new CompleteWorkoutDayCommand(
            workout.Id,
            new List<ExercisePerformanceDto>
            {
                new ExercisePerformanceDto(
                    exercise.Id,
                    new List<CompletedSetDto> { new CompletedSetDto(100m, 5) },
                    PerformanceResult.Success)
            });

        _unitOfWorkMock.Setup(x => x.Workouts.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.WeekCompleted.Should().BeTrue();
    }

    [Test]
    public async Task Handle_WithMissingExercisePerformances_ShouldThrowInvalidOperationException()
    {
        var workout = CreateTestWorkout();
        var command = new CompleteWorkoutDayCommand(
            workout.Id,
            new List<ExercisePerformanceDto>());

        _unitOfWorkMock.Setup(x => x.Workouts.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task Handle_WithSuccessfulPerformance_ShouldUpdateWorkoutProgress()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var initialDay = workout.CurrentDay;

        var command = new CompleteWorkoutDayCommand(
            workout.Id,
            new List<ExercisePerformanceDto>
            {
                new ExercisePerformanceDto(
                    exercise.Id,
                    new List<CompletedSetDto>
                    {
                        new CompletedSetDto(100m, 5),
                        new CompletedSetDto(100m, 5),
                        new CompletedSetDto(100m, 5)
                    },
                    PerformanceResult.Success)
            });

        _unitOfWorkMock.Setup(x => x.Workouts.GetByIdAsync(workout.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workout);

        var result = await _handler.Handle(command, CancellationToken.None);

        workout.CurrentDay.Should().Be(initialDay + 1);
    }

    private static Workout CreateTestWorkout()
    {
        var workoutName = WorkoutName.Create("Test Workout");
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();
        var exercises = new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Squat"),
                "hevy-squat",
                RestTimer.Create(180),
                1, 0, 3, workoutId,
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(140m),
                    WeightProgression.Create(2.5m),
                    2, true))
        };

        return Workout.Create(workoutName, userId, 5, exercises);
    }
}
