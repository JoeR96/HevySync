using System.Text.Json.Serialization;
using FluentValidation;
using HevySync.Endpoints.Average2Savage.Converters;

namespace HevySync.Endpoints.Average2Savage.Requests;

[JsonConverter(typeof(ExerciseDetailsRequestConverter))]
public abstract record ExerciseDetailsRequest;

public class ExerciseRequestValidator : AbstractValidator<ExerciseRequest>
{
    public ExerciseRequestValidator()
    {
        RuleFor(x => x.ExerciseName)
            .NotEmpty()
            .WithMessage("Exercise name cannot be empty.");

        RuleFor(x => x.Day)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Day must be greater than or equal to 1.");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Order must be greater than or equal to 1.");

        RuleFor(x => x.ExerciseDetailsRequest)
            .SetInheritanceValidator(v =>
            {
                v.Add(new LinearProgressionExerciseDetailsRequestValidator());
                v.Add(new RepsPerSetExerciseDetailsRequestValidator());
            })
            .When(x => x.ExerciseDetailsRequest is not null)
            .WithMessage("Exercise details are invalid.");
    }
}