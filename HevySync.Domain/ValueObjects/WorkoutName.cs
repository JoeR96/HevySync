using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class WorkoutName : ValueObject
{
    public string Value { get; }

    private WorkoutName(string value)
    {
        Value = value;
    }

    public static WorkoutName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Workout name cannot be empty", nameof(value));
        }

        if (value.Length > 200)
        {
            throw new ArgumentException("Workout name cannot exceed 200 characters", nameof(value));
        }

        return new WorkoutName(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(WorkoutName name) => name.Value;
}

