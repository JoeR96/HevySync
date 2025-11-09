using FluentAssertions;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using NUnit.Framework;
using InvalidWorkoutException = HevySync.Domain.Aggregates.InvalidWorkoutException;

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
        workout.Activity.WorkoutsInWeek.Should().Be(5);
        workout.Exercises.Should().HaveCount(1);
        workout.Activity.Week.Should().Be(1);
        workout.Activity.Day.Should().Be(1);
    }

    [Test]
    public void Create_WithEmptyExercises_ShouldThrowInvalidWorkoutException()
    {
        var workoutName = WorkoutName.Create("Test Workout");
        var userId = Guid.NewGuid();

        var act = () => Workout.Create(workoutName, userId, 5, new List<Exercise>());

        act.Should().Throw<InvalidWorkoutException>()
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

        workout.Activity.Day.Should().Be(2);
        workout.Activity.Week.Should().Be(1);
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

        workout.Activity.Week.Should().Be(2);
        workout.Activity.Day.Should().Be(1);
    }

    [Test]
    public void CompleteDay_WithMissingExercises_ShouldThrowInvalidWorkoutException()
    {
        var workout = CreateTestWorkout();

        var act = () => workout.CompleteDay(new List<ExercisePerformance>());

        act.Should().Throw<InvalidWorkoutException>()
            .WithMessage("*performances*");
    }

    [Test]
    public void ApplyProgression_WithSuccessfulPerformance_ShouldIncreaseTrainingMax()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var initialTrainingMax = (exercise.Progression as LinearProgressionStrategy)?.TrainingMax.Value;

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
        progression!.TrainingMax.Value.Should().BeGreaterThan(initialTrainingMax!.Value);
    }

    [Test]
    public void ApplyProgression_WithFailedPerformance_ShouldDecreaseTrainingMax()
    {
        var workout = CreateTestWorkout();
        var exercise = workout.Exercises.First();
        var initialTrainingMax = (exercise.Progression as LinearProgressionStrategy)?.TrainingMax.Value;

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
        progression!.TrainingMax.Value.Should().BeLessThan(initialTrainingMax!.Value);
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
