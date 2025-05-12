using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record LinearProgressionExerciseDetailsRequest(
    decimal WeightProgression,
    int AttemptsBeforeDeload,
    ExerciseProgram Program,
    BodyCategory BodyCategory,
    EquipmentType EquipmentType
) : ExerciseDetailsRequest;