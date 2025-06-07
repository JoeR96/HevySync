namespace HevySync.Endpoints.Responses;

public record ValidationErrorResponse(List<ValidationError> Errors);

public record ValidationError
{
    public string Property { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}