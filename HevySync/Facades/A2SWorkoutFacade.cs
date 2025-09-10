using HevySync.Models;
using HevySync.Models.Exercises;
using HevySync.Services;

namespace HevySync.Facades;

public class A2SWorkoutFacade(HypertrophyService hypertrophyService, RepsPerSetService repsPerSetService)
    : IA2SWorkoutFacade
{
    public async Task<List<Set>> CreateWeekOneSetsAsync(ExerciseDetail exercise,
        WorkoutActivity workoutActivity)
    {
        return exercise switch
        {
            LinearProgression linearProgression =>
                await hypertrophyService.CreateWeekOneSetsAsync(linearProgression, workoutActivity),
            RepsPerSet repsPerSet =>
                await repsPerSetService.CreateWeekOneSetsAsync(repsPerSet),
            _ => throw new ArgumentException($"Unsupported exercise type: {exercise.GetType().Name}")
        };
    }
}