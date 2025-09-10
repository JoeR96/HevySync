using FluentValidation;
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
    public async Task<Dictionary<int, List<SessionExercise>>> CreateWorkoutWeekOneAsync(
        SyncHevyWorkoutsRequest syncHevyWorkoutsRequest)
    {
        var workout = await dbContext.Workouts
            .Where(w => w.Id == syncHevyWorkoutsRequest.WorkoutId)
            .Include(workout => workout.WorkoutActivity)
            .Include(workout => workout.Exercises)
            .ThenInclude(exercise => exercise.ExerciseDetail)
            .FirstOrDefaultAsync();

        var groupedByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .ToDictionary(g => g.Key, g => g.ToList());

        var groupedTasks = groupedByDay.ToDictionary(
            group => group.Key,
            group => Task.WhenAll(
                group.Value.Select(async e => new SessionExercise
                {
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    RestSeconds = e.RestTimer,
                    Notes = e.ExerciseName,
                    Sets = await a2SWorkoutFacade.CreateWeekOneSetsAsync(e.ExerciseDetail, workout.WorkoutActivity)
                }))
        );

        var routineExercisesByDay = new Dictionary<int, List<SessionExercise>>();

        foreach (var (day, task) in
                 groupedTasks)
            routineExercisesByDay[day] = (await task).ToList();

        return routineExercisesByDay;
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

public class SessionExercise
{
    public string ExerciseTemplateId { get; set; }
    public int RestSeconds { get; set; }
    public string Notes { get; set; }
    public List<Set> Sets { get; set; }
}

public class SyncHevyWorkoutsRequest
{
    public Guid WorkoutId { get; set; }
}

public class SyncHevyWorkoutsRequestValidator : AbstractValidator<SyncHevyWorkoutsRequest>
{
    public SyncHevyWorkoutsRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithMessage("Workout Id cannot be empty.");
    }
}