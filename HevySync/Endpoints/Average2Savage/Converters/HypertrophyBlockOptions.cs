namespace HevySync.Configuration;

public class HypertrophyBlockOptions
{
    public LiftOptions Primary { get; set; } = new();
    public LiftOptions Auxiliary { get; set; } = new();
}