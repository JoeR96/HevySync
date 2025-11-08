using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class RestTimer : ValueObject
{
    public int Seconds { get; }

    private RestTimer(int seconds)
    {
        Seconds = seconds;
    }

    public static RestTimer Create(int seconds)
    {
        if (seconds < 0)
        {
            throw new ArgumentException("Rest timer cannot be negative", nameof(seconds));
        }

        if (seconds > 600) // 10 minutes max
        {
            throw new ArgumentException("Rest timer cannot exceed 10 minutes (600 seconds)", nameof(seconds));
        }

        return new RestTimer(seconds);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Seconds;
    }

    public override string ToString() => $"{Seconds}s";

    public static implicit operator int(RestTimer timer) => timer.Seconds;
}

