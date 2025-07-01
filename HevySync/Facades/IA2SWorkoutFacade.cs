using HevySync.Models;
using HevySync.Models.Exercises;
using HevySync.Services;

namespace HevySync.Facades;

public interface IA2SWorkoutFacade
{
    Task<List<RoutineSet>> CreateRoutineWeekOneSetsAsync(ExerciseDetail exercise, WorkoutActivity workoutActivity);
}