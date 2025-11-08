using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class ExerciseName : ValueObject
{
    public string Value { get; }

    private ExerciseName(string value)
    {
        Value = value;
    }

    public static ExerciseName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Exercise name cannot be empty", nameof(value));
        }

        if (value.Length > 200)
        {
            throw new ArgumentException("Exercise name cannot exceed 200 characters", nameof(value));
        }

        return new ExerciseName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ExerciseName name) => name.Value;
}

