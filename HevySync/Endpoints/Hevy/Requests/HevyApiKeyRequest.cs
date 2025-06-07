using FluentValidation;

namespace HevySync.Endpoints.Hevy.Requests;

public record HevyApiKeyRequest
{
    public string HevyApiKey { get; set; } = default!;
}

public class HevyApiKeyRequestValidator : AbstractValidator<HevyApiKeyRequest>
{
    public HevyApiKeyRequestValidator()
    {
        RuleFor(x => x.HevyApiKey)
            .NotEmpty()
            .WithMessage("The string cannot be empty.");
    }
}