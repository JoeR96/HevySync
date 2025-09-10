using HevySync.Models;
using HevySync.Models.Exercises;

namespace HevySync.Facades;

public interface IA2SWorkoutFacade
{
    Task<List<Set>> CreateWeekOneSetsAsync(ExerciseDetail exercise, WorkoutActivity workoutActivity);
}