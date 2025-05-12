using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Endpoints.Average2Savage.Responses;

public record RepsPerSetDto : ExerciseDetailDto
{
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int NumberOfSets { get; set; }
    public int TotalNumberOfSets { get; set; }
    public ExerciseProgram Program { get; set; }
}