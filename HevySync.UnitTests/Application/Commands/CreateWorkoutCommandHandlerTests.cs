using FluentAssertions;
using HevySync.Application.Workouts.Commands.CreateWorkout;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Repositories;
using Moq;
using NUnit.Framework;

namespace HevySync.UnitTests.Application.Commands;

[TestFixture]
public class CreateWorkoutCommandHandlerTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private CreateWorkoutCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        var workoutRepoMock = new Mock<IWorkoutRepository>();
        var activityRepoMock = new Mock<IRepository<Activity, Guid>>();

        _unitOfWorkMock.Setup(x => x.Workouts).Returns(workoutRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Activities).Returns(activityRepoMock.Object);

        _handler = new CreateWorkoutCommandHandler(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task Handle_WithValidCommand_ShouldCreateWorkoutAndActivity()
    {
        var command = CreateValidCommand();
        _unitOfWorkMock.Setup(x => x.Activities.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Workout");
        _unitOfWorkMock.Verify(x => x.Workouts.AddAsync(It.IsAny<Workout>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Activities.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithExistingActiveActivity_ShouldThrowInvalidOperationException()
    {
        var command = CreateValidCommand();
        var activityRepoMock = new Mock<IRepository<Activity, Guid>>();

        activityRepoMock.Setup(x => x.AnyAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.Activities).Returns(activityRepoMock.Object);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has an active*");
    }

    [Test]
    public async Task Handle_WithLinearProgressionExercise_ShouldCreateCorrectly()
    {
        var command = new CreateWorkoutCommand(
            "Linear Progression Workout",
            Guid.NewGuid(),
            5,
            new List<CreateExerciseDto>
            {
                new CreateExerciseDto(
                    "Squat",
                    "hevy-squat",
                    180,
                    1, 0, 3,
                    new CreateLinearProgressionDto(140m, 2.5m, 2, true))
            });

        _unitOfWorkMock.Setup(x => x.Activities.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Exercises.Should().HaveCount(1);
        result.Exercises.First().Name.Should().Be("Squat");
    }

    [Test]
    public async Task Handle_WithRepsPerSetExercise_ShouldCreateCorrectly()
    {
        var command = new CreateWorkoutCommand(
            "Reps Per Set Workout",
            Guid.NewGuid(),
            5,
            new List<CreateExerciseDto>
            {
                new CreateExerciseDto(
                    "Romanian Deadlift",
                    "hevy-rdl",
                    120,
                    1, 1, 4,
                    new CreateRepsPerSetDto(8, 10, 12, 3, 5, 80m, 2.5m))
            });

        _unitOfWorkMock.Setup(x => x.Activities.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Exercises.Should().HaveCount(1);
        result.Exercises.First().Name.Should().Be("Romanian Deadlift");
    }

    [Test]
    public async Task Handle_WithMultipleExercises_ShouldCreateAll()
    {
        var command = new CreateWorkoutCommand(
            "Multi Exercise Workout",
            Guid.NewGuid(),
            5,
            new List<CreateExerciseDto>
            {
                new CreateExerciseDto(
                    "Squat",
                    "hevy-squat",
                    180,
                    1, 0, 3,
                    new CreateLinearProgressionDto(140m, 2.5m, 2, true)),
                new CreateExerciseDto(
                    "Romanian Deadlift",
                    "hevy-rdl",
                    120,
                    1, 1, 4,
                    new CreateRepsPerSetDto(8, 10, 12, 3, 5, 80m, 2.5m))
            });

        _unitOfWorkMock.Setup(x => x.Activities.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Exercises.Should().HaveCount(2);
    }

    private static CreateWorkoutCommand CreateValidCommand() =>
        new CreateWorkoutCommand(
            "Test Workout",
            Guid.NewGuid(),
            5,
            new List<CreateExerciseDto>
            {
                new CreateExerciseDto(
                    "Squat",
                    "hevy-squat",
                    180,
                    1, 0, 3,
                    new CreateLinearProgressionDto(140m, 2.5m, 2, true))
            });
}
