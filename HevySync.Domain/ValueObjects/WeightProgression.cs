using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class WeightProgression : ValueObject
{
    public decimal Value { get; }

    private WeightProgression(decimal value)
    {
        Value = value;
    }

    public static WeightProgression Create(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Weight progression must be greater than zero", nameof(value));
        }

        return new WeightProgression(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"+{Value}kg";

    public static implicit operator decimal(WeightProgression progression) => progression.Value;
}

