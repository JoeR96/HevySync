using HevySync.Models;
using HevySync.Models.Exercises;

namespace HevySync.Services;

public class RepsPerSetService
{
    public async Task<List<RoutineSet>> CreateRoutineWeekOneSetsAsync(
        RepsPerSet exercise)
    {
        var sets = new List<RoutineSet>();
        for (var i = 0; i < exercise.StartingSetCount; i++)
            sets.Add(new RoutineSet
            {
                WeightKg = (double?)exercise.StartingWeight,
                Reps = exercise.MinimumReps,
                Type = "normal"
            });

        return sets;
    }
}