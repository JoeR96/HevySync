using FluentValidation;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record GenerateWeekOneRequest
{
    public Guid WorkoutId { get; set; }
}

public class GenerateWeekOneRequestValidator : AbstractValidator<GenerateWeekOneRequest>
{
    public GenerateWeekOneRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithMessage("Workout ID cannot be empty.");
    }
}

