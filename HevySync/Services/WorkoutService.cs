using HevySync.Data;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Facades;
using HevySync.Models;
using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Services;

public class WorkoutService(
    HevySyncDbContext dbContext,
    IA2SWorkoutFacade a2SWorkoutFacade)
{
    public async Task<WeeklyWorkoutPlan> CreateWorkoutWeekOneAsync(
        SyncHevyWorkoutsRequest syncHevyWorkoutsRequest)
    {
        var workout = await GetWorkoutWithDetailsAsync(syncHevyWorkoutsRequest.WorkoutId);

        var dailyWorkouts = await CreateDailyWorkoutsAsync(workout);

        await SaveSessionExercisesToDatabaseAsync(dailyWorkouts);

        return new WeeklyWorkoutPlan
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name,
            Week = workout.WorkoutActivity.Week,
            DailyWorkouts = dailyWorkouts
        };
    }

    private async Task<Workout> GetWorkoutWithDetailsAsync(Guid workoutId)
    {
        var workout = await dbContext.Workouts
            .Where(w => w.Id == workoutId)
            .Include(w => w.WorkoutActivity)
            .Include(w => w.Exercises)
            .ThenInclude(e => e.ExerciseDetail)
            .FirstOrDefaultAsync();

        if (workout == null)
            throw new InvalidOperationException($"Workout with ID {workoutId} not found.");

        return workout;
    }

    private async Task<List<DailyWorkout>> CreateDailyWorkoutsAsync(Workout workout)
    {
        var exercisesByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .OrderBy(g => g.Key)
            .ToList();

        var dailyWorkouts = new List<DailyWorkout>();

        foreach (var dayGroup in exercisesByDay)
        {
            var sessionExercises = await CreateSessionExercisesForDayAsync(
                dayGroup.ToList(),
                workout);

            dailyWorkouts.Add(new DailyWorkout
            {
                Day = dayGroup.Key,
                SessionExercises = sessionExercises
            });
        }

        return dailyWorkouts;
    }

    private async Task<List<SessionExercise>> CreateSessionExercisesForDayAsync(
        List<Exercise> exercises,
        Workout workout)
    {
        var sessionExercises = new List<SessionExercise>();

        foreach (var exercise in exercises.OrderBy(e => e.Order))
        {
            var sets = await a2SWorkoutFacade.CreateWeekOneSetsAsync(
                exercise.ExerciseDetail,
                workout.WorkoutActivity);

            sessionExercises.Add(new SessionExercise
            {
                Sets = sets,
                Exercise = exercise,
                ExerciseId = exercise.Id
            });
        }

        return sessionExercises;
    }

    private async Task SaveSessionExercisesToDatabaseAsync(List<DailyWorkout> dailyWorkouts)
    {
        var allSessionExercises = dailyWorkouts
            .SelectMany(dw => dw.SessionExercises)
            .ToList();

        dbContext.SessionExercises.AddRange(allSessionExercises);
        await dbContext.SaveChangesAsync();
    }

    private IEnumerable<RoutineSet> CreateRoutineSets(
        Exercise exercise,
        WorkoutActivity workoutActivity)
    {
        if (exercise?.ExerciseDetail == null)
            throw new ArgumentNullException(nameof(exercise), "Exercise or ExerciseDetail cannot be null.");

        for (var i = 0; i < exercise.NumberOfSets; i++)
            switch (exercise.ExerciseDetail)
            {
                case LinearProgression:
                    yield return new RoutineSet
                    {
                        WeightKg = 0,
                        Reps = 0
                    };
                    break;

                case RepsPerSet reps:
                    yield return new RoutineSet
                    {
                        WeightKg = reps.TargetReps,
                        Reps = reps.TargetReps
                    };
                    break;

                default:
                    throw new NotImplementedException(
                        $"Unhandled ExerciseDetail type: {exercise.ExerciseDetail.GetType().Name}");
            }
    }
}