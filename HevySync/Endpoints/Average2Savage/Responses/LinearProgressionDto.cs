using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Models.Exercises;

namespace HevySync.Endpoints.Average2Savage.Responses;

public record LinearProgressionDto : ExerciseDetailDto
{
    public decimal WeightProgression { get; set; }
    public int AttemptsBeforeDeload { get; set; }
    public BodyCategory BodyCategory { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public ExerciseProgram Program { get; set; }
    public decimal TrainingMax { get; set; }
}