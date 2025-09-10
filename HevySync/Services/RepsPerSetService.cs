using HevySync.Models.Exercises;

namespace HevySync.Services;

public class RepsPerSetService
{
    public async Task<List<Set>> CreateWeekOneSetsAsync(
        RepsPerSet exercise)
    {
        var sets = new List<Set>();
        for (var i = 0; i < exercise.StartingSetCount; i++)
            sets.Add(new Set
            {
                WeightKg = exercise.StartingWeight,
                Reps = exercise.MinimumReps
            });

        return sets;
    }
}