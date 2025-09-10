using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Models.Exercises;

namespace HevySync.Endpoints.Average2Savage.Responses;

public record ExerciseDto
{
    public Guid Id { get; set; }
    public string ExerciseName { get; set; }
    public int Day { get; set; }
    public int RestTimer { get; set; }
    public ExerciseDetailDto ExerciseDetail { get; set; }
    public int Order { get; set; }
    public int NumberOfSets { get; set; }
}

public static class ExerciseDetailExtensions
{
    public static ExerciseDetailDto ToDto(this ExerciseDetail exerciseDetail)
    {
        return exerciseDetail switch
        {
            LinearProgression lp => new LinearProgressionDto
            {
                Program = ExerciseProgram.Average2SavageHypertrophy,
                Id = lp.Id,
                WeightProgression = lp.WeightProgression,
                AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                TrainingMax = lp.TrainingMax
            },
            RepsPerSet rps => new RepsPerSetDto
            {
                StartingWeight = rps.StartingWeight,
                Program = ExerciseProgram.Average2SavageRepsPerSet,
                Id = rps.Id,
                MinimumReps = rps.MinimumReps,
                TargetReps = rps.TargetReps,
                MaximumTargetReps = rps.MaximumTargetReps,
                StartingSetCount = rps.StartingSetCount,
                TargetSetCount = rps.TargetSetCount
            },
            _ => throw new InvalidOperationException($"Unknown exercise detail type: {exerciseDetail?.GetType().Name}")
        };
    }
}