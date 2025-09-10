namespace HevySync.Configuration;

public class HypertrophyOptions
{
    public const string SectionName = "Hypertrophy";

    public decimal DefaultRoundingValue { get; set; }
    public HypertrophyBlockOptions HypertrophyBlock { get; set; } = new();
}