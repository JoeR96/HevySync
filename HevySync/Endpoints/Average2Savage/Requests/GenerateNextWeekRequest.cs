using FluentValidation;

namespace HevySync.Endpoints.Average2Savage.Requests;

public sealed record GenerateNextWeekRequest
{
    public Guid WorkoutId { get; init; }
    public List<ExercisePerformanceRequest> WeekPerformances { get; init; } = new();
}

public sealed class GenerateNextWeekRequestValidator : AbstractValidator<GenerateNextWeekRequest>
{
    public GenerateNextWeekRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithMessage("WorkoutId is required");

        RuleFor(x => x.WeekPerformances)
            .NotEmpty()
            .WithMessage("Week performances are required");

        RuleForEach(x => x.WeekPerformances)
            .SetValidator(new ExercisePerformanceRequestValidator());
    }
}

