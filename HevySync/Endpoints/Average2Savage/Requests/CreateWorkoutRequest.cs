using FluentValidation;

namespace HevySync.Endpoints.Average2Savage.Requests;

public record CreateWorkoutRequest
{
    public string WorkoutName { get; set; }
    public List<ExerciseRequest> Exercises { get; set; }
    public int WorkoutDaysInWeek { get; set; }
}

public class CreateWorkoutRequestValidator : AbstractValidator<CreateWorkoutRequest>
{
    public CreateWorkoutRequestValidator()
    {
        RuleFor(x => x.WorkoutName)
            .NotEmpty()
            .WithMessage("Workout name cannot be empty.");

        RuleForEach(x => x.Exercises)
            .SetValidator(new ExerciseRequestValidator())
            .WithMessage("Each exercise must be valid.");

        RuleFor(x => x.Exercises)
            .NotEmpty()
            .WithMessage("A workout must contain at least one exercise.");
    }
}