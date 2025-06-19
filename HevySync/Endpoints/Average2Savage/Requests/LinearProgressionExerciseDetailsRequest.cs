using FluentValidation;
using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record LinearProgressionExerciseDetailsRequest(
    decimal TrainingMax,
    decimal WeightProgression,
    int AttemptsBeforeDeload,
    ExerciseProgram Program,
    BodyCategory BodyCategory,
    EquipmentType EquipmentType
) : ExerciseDetailsRequest;

public class
    LinearProgressionExerciseDetailsRequestValidator : AbstractValidator<LinearProgressionExerciseDetailsRequest>
{
    public LinearProgressionExerciseDetailsRequestValidator()
    {
        RuleFor(x => x.TrainingMax)
            .GreaterThan(0)
            .NotEmpty()
            .NotNull()
            .WithMessage("Working weight must be greater than 0.");

        RuleFor(x => x.WeightProgression)
            .GreaterThan(0)
            .WithMessage("Weight progression must be greater than 0.");

        RuleFor(x => x.AttemptsBeforeDeload)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Attempts before deload must be at least 1.");
    }
}