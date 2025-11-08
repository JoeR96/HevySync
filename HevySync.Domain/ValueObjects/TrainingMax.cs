using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class TrainingMax : ValueObject
{
    public decimal Value { get; }

    private TrainingMax(decimal value)
    {
        Value = value;
    }

    public static TrainingMax Create(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Training max must be greater than zero", nameof(value));
        }

        return new TrainingMax(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}kg";

    public static implicit operator decimal(TrainingMax trainingMax) => trainingMax.Value;
}

