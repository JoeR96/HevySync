using FluentValidation;

namespace HevySync.Endpoints.Average2Savage.Requests;

public class SyncHevyWorkoutsRequest
{
    public Guid WorkoutId { get; set; }
}

public class SyncHevyWorkoutsRequestValidator : AbstractValidator<SyncHevyWorkoutsRequest>
{
    public SyncHevyWorkoutsRequestValidator()
    {
        RuleFor(x => x.WorkoutId)
            .NotEmpty()
            .WithMessage("Workout Id cannot be empty.");
    }
}