using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Models.Exercises;

namespace HevySync.Endpoints.Average2Savage.Responses;

public record RepsPerSetDto : ExerciseDetailDto
{
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int StartingSetCount { get; set; }
    public int TargetSetCount { get; set; }
    public ExerciseProgram Program { get; set; }
    public decimal StartingWeight { get; set; }
    public decimal WeightProgression { get; set; }
}