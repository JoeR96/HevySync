using HevySync.Configuration;
using HevySync.Models;
using HevySync.Models.Exercises;
using Microsoft.Extensions.Options;

namespace HevySync.Services;

public class HypertrophyService(IOptions<HypertrophyOptions> options)
{
    private readonly HypertrophyOptions _options = options.Value;

    public async Task<List<Set>> CreateWeekOneSetsAsync(
        LinearProgression exercise,
        WorkoutActivity workoutActivity)
    {
        var sets = new List<Set>();
        var config = exercise.Primary ? _options.HypertrophyBlock.Primary : _options.HypertrophyBlock.Auxiliary;

        for (var i = 0; i < config.Sets; i++)
            sets.Add(new Set
            {
                WeightKg = GetWorkingWeight(workoutActivity.Week, exercise.Primary, exercise.TrainingMax),
                Reps = config.RepsPerSet[workoutActivity.Week]
            });

        sets.Last().Reps = config.AmrapRepTarget[workoutActivity.Week];
        return sets;
    }

    private decimal GetWorkingWeight(int week, bool primary, decimal trainingMax)
    {
        var config = primary ? _options.HypertrophyBlock.Primary : _options.HypertrophyBlock.Auxiliary;
        var workingWeight = config.Intensity[week] * trainingMax;
        var newWeight = Math.Round(workingWeight / _options.DefaultRoundingValue);
        return newWeight * _options.DefaultRoundingValue;
    }
}