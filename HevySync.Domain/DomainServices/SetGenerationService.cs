using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.DomainServices;

public class SetGenerationService : ISetGenerationService
{
    public Task<List<Set>> GenerateWeekOneSetsAsync(
        ExerciseProgression progression,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default)
    {
        var sets = progression switch
        {
            LinearProgressionStrategy lp => GenerateLinearProgressionSets(lp, activity),
            RepsPerSetStrategy rps => GenerateRepsPerSetSets(rps),
            _ => throw new ArgumentException($"Unsupported progression type: {progression.GetType().Name}")
        };

        return Task.FromResult(sets);
    }

    private static List<Set> GenerateLinearProgressionSets(LinearProgressionStrategy progression, WorkoutActivity activity)
    {
        var sets = new List<Set>();
        
        // A2S Hypertrophy configuration
        var numberOfSets = progression.IsPrimary ? 3 : 3; // 3 sets for both primary and auxiliary
        
        // Week intensity pattern: 70%, 75%, 80%, 70%, 75%, 80% (repeating)
        var weekIntensities = new[] { 0.70m, 0.75m, 0.80m, 0.70m, 0.75m, 0.80m };
        var intensity = weekIntensities[(activity.Week - 1) % 6];
        
        // Rep targets: 10, 8, 6, 9, 7, 5 (repeating)
        var repTargets = new[] { 10, 8, 6, 9, 7, 5 };
        var repsPerSet = repTargets[(activity.Week - 1) % 6];
        var amrapTarget = repsPerSet; // Last set is AMRAP with same target
        
        // Calculate working weight
        var workingWeight = progression.TrainingMax.Value * intensity;
        var roundedWeight = Math.Round(workingWeight / 2.5m) * 2.5m; // Round to nearest 2.5kg
        
        // Generate sets (all but last are straight sets)
        for (int i = 0; i < numberOfSets - 1; i++)
        {
            sets.Add(Set.Create(roundedWeight, repsPerSet));
        }
        
        // Last set is AMRAP
        sets.Add(Set.Create(roundedWeight, amrapTarget));
        
        return sets;
    }

    private static List<Set> GenerateRepsPerSetSets(RepsPerSetStrategy progression)
    {
        var sets = new List<Set>();
        
        // Use starting set count and minimum reps
        for (int i = 0; i < progression.StartingSetCount; i++)
        {
            sets.Add(Set.Create(progression.StartingWeight, progression.RepRange.MinimumReps));
        }
        
        return sets;
    }
}
