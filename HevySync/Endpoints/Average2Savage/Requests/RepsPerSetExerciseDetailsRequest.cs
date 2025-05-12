using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record RepsPerSetExerciseDetailsRequest(
    int MinimumReps,
    int TargetReps,
    int MaximumTargetReps,
    int NumberOfSets,
    int TotalNumberOfSets,
    ExerciseProgram Program
) : ExerciseDetailsRequest;