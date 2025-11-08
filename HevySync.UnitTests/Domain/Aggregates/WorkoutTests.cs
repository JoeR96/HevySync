using FluentAssertions;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;
using NUnit.Framework;

namespace HevySync.UnitTests.Domain.Aggregates;

[TestFixture]
public class WorkoutTests
{
    [Test]
    public void Create_WithValidParameters_ShouldCreateWorkout()
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
                    TrainingMax.Create(100m),
                    WeightProgression.Create(2.5m),
                    2, true))
        };

        var workout = Workout.Create(workoutName, userId, 5, exercises);

        workout.Should().NotBeNull();
        workout.Name.Should().Be(workoutName);
        workout.UserId.Should().Be(userId);
        workout.WorkoutDaysInWeek.Should().Be(5);
        workout.Exercises.Should().HaveCount(1);
        workout.CurrentWeek.Should().Be(1);
        workout.CurrentDay.Should().Be(1);
    }

    [Test]
    public void Create_WithEmptyExercises_ShouldThrowArgumentException()
    {
        var workoutName = WorkoutName.Create("Test Workout");
        var userId = Guid.NewGuid();

        var act = () => Workout.Create(workoutName, userId, 5, new List<Exercise>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*at least one exercise*");
    }

    [Test]
    public void Create_WithInvalidDaysInWeek_ShouldThrowArgumentException()
    {
        var workoutName = WorkoutName.Create("Test Workout");
        var userId = Guid.NewGuid();
        var exercises = new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Squat"),
                "hevy-squat",
                RestTimer.Create(180),
                1, 0, 3, Guid.NewGuid(),
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(100m),
                    WeightProgression.Create(2.5m),
                    2, true))
        };

        var act = () => Workout.Create(workoutName, userId, 0, exercises);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*must be between 1 and 7*");
    }

    [Test]
    public void CompleteDay_WithValidPerformances_ShouldAdvanceToNextDay()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var performances = new List<ExercisePerformance>
        {
            ExercisePerformance.Create(
                exercise.Id,
                new List<Set> { Set.Create(100m, 5), Set.Create(100m, 5), Set.Create(100m, 5) },
                PerformanceResult.Success)
        };

        workout.CompleteDay(performances);

        workout.CurrentDay.Should().Be(2);
        workout.CurrentWeek.Should().Be(1);
    }

    [Test]
    public void CompleteDay_OnLastDayOfWeek_ShouldAdvanceToNextWeek()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var performances = new List<ExercisePerformance>
        {
            ExercisePerformance.Create(
                exercise.Id,
                new List<Set> { Set.Create(100m, 5) },
                PerformanceResult.Success)
        };

        for (int i = 0; i < 5; i++)
        {
            workout.CompleteDay(performances);
        }

        workout.CurrentWeek.Should().Be(1);
        workout.CurrentDay.Should().Be(5);
    }

    [Test]
    public void CompleteDay_WithMissingExercises_ShouldThrowInvalidOperationException()
    {
        var workout = CreateTestWorkout();

        var act = () => workout.CompleteDay(new List<ExercisePerformance>());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*performances for all exercises*");
    }

    [Test]
    public void ApplyProgression_WithSuccessfulPerformance_ShouldIncreaseWeights()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var initialWeight = (exercise.Progression as LinearProgressionStrategy)?.CurrentWeight;

        var performances = new List<ExercisePerformance>
        {
            ExercisePerformance.Create(
                exercise.Id,
                new List<Set> { Set.Create(100m, 5), Set.Create(100m, 5), Set.Create(100m, 5) },
                PerformanceResult.Success)
        };

        workout.ApplyProgression(performances);

        var progression = exercise.Progression as LinearProgressionStrategy;
        progression.Should().NotBeNull();
        progression!.CurrentWeight.Should().BeGreaterThan(initialWeight!.Value);
    }

    [Test]
    public void ApplyProgression_WithFailedPerformance_ShouldDecrementAttempts()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var initialAttempts = (exercise.Progression as LinearProgressionStrategy)?.CurrentAttempts;

        var performances = new List<ExercisePerformance>
        {
            ExercisePerformance.Create(
                exercise.Id,
                new List<Set> { Set.Create(100m, 3) },
                PerformanceResult.Failed)
        };

        workout.ApplyProgression(performances);

        var progression = exercise.Progression as LinearProgressionStrategy;
        progression.Should().NotBeNull();
        progression!.CurrentAttempts.Should().BeLessThan(initialAttempts!.Value);
    }

    [Test]
    public void GetExercisesForDay_WithValidDay_ShouldReturnCorrectExercises()
    {
        var workout = CreateTestWorkout();

        var exercises = workout.GetExercisesForDay(1);

        exercises.Should().HaveCount(1);
        exercises.First().Day.Should().Be(1);
    }

    [Test]
    public void GetExercisesForDay_WithInvalidDay_ShouldReturnEmptyList()
    {
        var workout = CreateTestWorkout();

        var exercises = workout.GetExercisesForDay(99);

        exercises.Should().BeEmpty();
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
