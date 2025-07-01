using HevySync.Data;
using HevySync.Facades;
using HevySync.Models;
using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Services;

public class WorkoutService(
    HevyApiService hevyApiService,
    HevySyncDbContext dbContext,
    IA2SWorkoutFacade a2SWorkoutFacade)
{
    public async Task<List<RoutineResponse>> CreateHevyWorkoutWeekOneAsync(
        SyncHevyWorkoutsRequest syncHevyWorkoutsRequest)
    {
        var workout = await dbContext.Workouts.Where(w => w.Id == syncHevyWorkoutsRequest.WorkoutId)`.FirstOrDefaultAsync();

        var workoutActivity = workout.WorkoutActivity;

        var routineFolders = await hevyApiService.GetRoutineFoldersAsync();
        int? Average2SavageFolder = routineFolders.RoutineFolders.FirstOrDefault(f => f.Title == "Average2Savage").Id;

        var groupedByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .ToDictionary(g => g.Key, g => g.ToList());

        var groupedTasks = groupedByDay.ToDictionary(
            group => group.Key,
            group => Task.WhenAll(
                group.Value.Select(async e => new RoutineExercise
                {
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    RestSeconds = e.RestTimer,
                    Notes = e.ExerciseName,
                    Sets = await a2SWorkoutFacade.CreateRoutineWeekOneSetsAsync(e.ExerciseDetail, workoutActivity)
                }))
        );

        var routineExercisesByDay = new Dictionary<int, List<RoutineExercise>>();

        foreach (var (day, task) in
                 groupedTasks)
            routineExercisesByDay[day] = (await task).ToList();


        List<RoutineResponse> routineExercises = new();
        foreach (var (day, exercises) in routineExercisesByDay)
        {
            // Create a routine request for each day
            var routineRequest = new RoutineRequest
            {
                Routine = new Routine
                {
                    Notes = $"Week {workoutActivity.Week} of the Average2Savage routine - Day {day}",
                    FolderId = Average2SavageFolder, // Assuming this is a predefined folder ID
                    Title =
                        $"Average2Savage Week {workoutActivity.Week} Day {day}", // Customize based on current week and day
                    Exercises = exercises // List of exercises for the specific day
                }
            };

            Console.WriteLine($"Creating routine for Day {day}...");

            // Asynchronously call the Hevy API service to create the routine
            var response = await hevyApiService.CreateRoutineAsync(routineRequest);
            routineExercises.Add(response);
            Console.WriteLine($"Routine for Day {day} successfully created! Title: {routineRequest.Routine.Title}");
        }

        return routineExercises;
    }

    private IEnumerable<RoutineSet> CreateRoutineSets(
        Exercise exercise,
        WorkoutActivity workoutActivity)
    {
        if (exercise == null || exercise.ExerciseDetail == null)
            throw new ArgumentNullException(nameof(exercise), "Exercise or ExerciseDetail cannot be null.");

        for (var i = 0; i < exercise.NumberOfSets; i++)
            switch (exercise.ExerciseDetail)
            {
                case LinearProgression reps:
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

public class SyncHevyWorkoutsRequest
{
    public Guid WorkoutId { get; set; }
}