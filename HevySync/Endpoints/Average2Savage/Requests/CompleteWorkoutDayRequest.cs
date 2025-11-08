using FluentValidation;

namespace HevySync.Endpoints.Average2Savage.Requests;

public sealed record CompleteWorkoutDayRequest
{
    public Guid WorkoutId { get; init; }
    public List<ExercisePerformanceRequest> ExercisePerformances { get; init; } = new();
}

public sealed record ExercisePerformanceRequest
{
    public Guid ExerciseId { get; init; }
    public List<CompletedSetRequest> CompletedSets { get; init; } = new();
    public string PerformanceResult { get; init; } = string.Empty; // "Success", "Maintained", "Failed"
}

public sealed record CompletedSetRequest
{
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
}

public sealed class CompleteWorkoutDayRequestValidator : AbstractValidator<CompleteWorkoutDayRequest>
{
    public CompleteWorkoutDayRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithMessage("WorkoutId is required");

        RuleFor(x => x.ExercisePerformances)
            .NotEmpty()
            .WithMessage("At least one exercise performance is required");

        RuleForEach(x => x.ExercisePerformances)
            .SetValidator(new ExercisePerformanceRequestValidator());
    }
}

public sealed class ExercisePerformanceRequestValidator : AbstractValidator<ExercisePerformanceRequest>
{
    public ExercisePerformanceRequestValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty()
            .WithMessage("ExerciseId is required");

        RuleFor(x => x.CompletedSets)
            .NotEmpty()
            .WithMessage("At least one completed set is required");

        RuleFor(x => x.PerformanceResult)
            .NotEmpty()
            .WithMessage("PerformanceResult is required")
            .Must(x => x == "Success" || x == "Maintained" || x == "Failed")
            .WithMessage("PerformanceResult must be 'Success', 'Maintained', or 'Failed'");

        RuleForEach(x => x.CompletedSets)
            .SetValidator(new CompletedSetRequestValidator());
    }
}

public sealed class CompletedSetRequestValidator : AbstractValidator<CompletedSetRequest>
{
    public CompletedSetRequestValidator()
    {
        RuleFor(x => x.WeightKg)
            .GreaterThan(0)
            .WithMessage("WeightKg must be greater than 0");

        RuleFor(x => x.Reps)
            .GreaterThan(0)
            .WithMessage("Reps must be greater than 0");
    }
}

