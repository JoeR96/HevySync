using HevySync.Models;
using HevySync.Models.Exercises;

namespace HevySync.Services;

public class HypertrophyService
{
    public HypertrophyService()
    {
        a2SPrimaryLifts = new Dictionary<A2SBlocks, A2SBlockTemplateValue>
        {
            { A2SBlocks.Hypertrophy, HTPL }
        };

        a2SAuxLifts = new Dictionary<A2SBlocks, A2SBlockTemplateValue>
        {
            { A2SBlocks.Hypertrophy, HTAL }
        };
    }

    private Dictionary<A2SBlocks, A2SBlockTemplateValue> a2SPrimaryLifts { get; }
    private Dictionary<A2SBlocks, A2SBlockTemplateValue> a2SAuxLifts { get; }
    private HyperTrophyBlockPrimaryLift HTPL { get; } = new();
    private HyperTrophyBlockAuxillaryLift HTAL { get; } = new();

    public async Task<List<Set>> CreateWeekOneSetsAsync(
        LinearProgression exercise,
        WorkoutActivity workoutActivity)
    {
        var sets = new List<Set>();
        for (var i = 0; i < GetSets(A2SBlocks.Hypertrophy, workoutActivity.Week, exercise.Primary); i++)
            sets.Add(new Set
            {
                WeightKg = GetWorkingWeight(A2SBlocks.Hypertrophy, workoutActivity.Week, exercise.Primary,
                    exercise.TrainingMax, 1.25m),
                Reps = GetRepsPerSet(A2SBlocks.Hypertrophy, workoutActivity.Week, exercise.Primary)
            });

        sets.Last().Reps = GetAmprapRepTarget(A2SBlocks.Hypertrophy, workoutActivity.Week, exercise.Primary);
        return sets;
    }

    public int GetRepsPerSet(A2SBlocks block, int week, bool primary)
    {
        return primary ? a2SPrimaryLifts[block].repsPerSet[week] : a2SAuxLifts[block].repsPerSet[week];
    }

    public decimal GetIntensity(A2SBlocks block, int week, bool primary)
    {
        return primary ? a2SPrimaryLifts[block].intensity[week] : a2SAuxLifts[block].intensity[week];
    }

    public int GetSets(A2SBlocks block, int week, bool primary)
    {
        return primary ? a2SPrimaryLifts[block].sets : a2SAuxLifts[block].sets;
    }

    public decimal GetWorkingWeight(A2SBlocks block, int week, bool primary, decimal trainingMax, decimal roundingValue)
    {
        var workingWeight = GetIntensity(block, week, primary) * trainingMax;
        var newWeight = Math.Round(workingWeight / roundingValue);
        return newWeight * roundingValue;
    }

    public int GetAmprapRepTarget(A2SBlocks block, int week, bool primary)
    {
        return primary ? a2SPrimaryLifts[block].amrapRepTarget[week] : a2SAuxLifts[block].amrapRepTarget[week];
    }
}

public enum A2SBlocks
{
    Hypertrophy,
    Strength,
    Peaking
}

public enum ExerciseCompletedStatus
{
    Progressed = 1,
    StayedTheSame = 2,
    Failed = 3,
    Deload = 4,
    Active = 5,
    NonProgressable
}

public abstract class A2SBlockTemplateValue
{
    public int[] amrapRepTarget { get; set; }
    public int[] repsPerSet { get; set; }
    public decimal[] intensity { get; set; }
    public int sets { get; set; }
    public bool aux { get; set; }
}

public class HyperTrophyBlockPrimaryLift : A2SBlockTemplateValue
{
    public HyperTrophyBlockPrimaryLift()
    {
        amrapRepTarget = new[] { 10, 8, 6, 9, 7, 5 };
        repsPerSet = new[] { 5, 4, 3, 5, 4, 3 };
        intensity = new[] { 0.70m, 0.75m, 0.80m, 0.725m, 0.775m, 0.825m };
        sets = 5;
        aux = false;
    }
}

public class HyperTrophyBlockAuxillaryLift : A2SBlockTemplateValue
{
    public HyperTrophyBlockAuxillaryLift()
    {
        amrapRepTarget = new[] { 14, 12, 10, 13, 11, 9 };
        repsPerSet = new[] { 7, 6, 5, 7, 6, 5 };
        intensity = new[] { 0.60m, 0.65m, 0.70m, 0.625m, 0.675m, 0.725m };
        sets = 3;
        aux = true;
    }
}