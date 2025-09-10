namespace HevySync.Configuration;

public class HypertrophyOptions
{
    public const string SectionName = "Hypertrophy";

    public decimal DefaultRoundingValue { get; set; }
    public HypertrophyBlockOptions HypertrophyBlock { get; set; } = new();
}

public class HypertrophyBlockOptions
{
    public LiftOptions Primary { get; set; } = new();
    public LiftOptions Auxiliary { get; set; } = new();
}

public class LiftOptions
{
    public int[] AmrapRepTarget { get; set; } = Array.Empty<int>();
    public int[] RepsPerSet { get; set; } = Array.Empty<int>();
    public decimal[] Intensity { get; set; } = Array.Empty<decimal>();
    public int Sets { get; set; }
}