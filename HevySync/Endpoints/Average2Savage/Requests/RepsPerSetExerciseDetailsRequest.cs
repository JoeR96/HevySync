using FluentValidation;
using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record RepsPerSetExerciseDetailsRequest(
    int MinimumReps,
    int TargetReps,
    int MaximumTargetReps,
    int NumberOfSets,
    int TotalNumberOfSets,
    decimal StartingWeight,
    ExerciseProgram Program
) : ExerciseDetailsRequest;

public class RepsPerSetExerciseDetailsRequestValidator : AbstractValidator<RepsPerSetExerciseDetailsRequest>
{
    public RepsPerSetExerciseDetailsRequestValidator()
    {
        RuleFor(x => x.StartingWeight)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Starting weight must be greater than or equal to 0.");

        RuleFor(x => x.MinimumReps)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Minimum reps must be at least 1.");

        RuleFor(x => x.TargetReps)
            .GreaterThan(0)
            .WithMessage("Target reps must be greater than 0.");

        RuleFor(x => x.MaximumTargetReps)
            .GreaterThan(x => x.TargetReps)
            .WithMessage("Maximum target reps must be greater than target reps.");

        RuleFor(x => x.NumberOfSets)
            .GreaterThan(0)
            .WithMessage("Number of sets must be greater than 0.");

        RuleFor(x => x.TotalNumberOfSets)
            .GreaterThanOrEqualTo(x => x.NumberOfSets)
            .WithMessage("Total number of sets must be greater than or equal to number of sets.");
    }
}