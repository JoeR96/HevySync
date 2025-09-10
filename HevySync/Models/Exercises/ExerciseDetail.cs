using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Converters;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Responses;

namespace HevySync.Models.Exercises;

public abstract class ExerciseDetail
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
}

[JsonConverter(typeof(ExerciseDetailDtoConverter))]
public record ExerciseDetailDto
{
    public Guid Id { get; set; }
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