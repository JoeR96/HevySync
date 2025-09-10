namespace HevySync.Configuration;

public class LiftOptions
{
    public int[] AmrapRepTarget { get; set; } = Array.Empty<int>();
    public int[] RepsPerSet { get; set; } = Array.Empty<int>();
    public decimal[] Intensity { get; set; } = Array.Empty<decimal>();
    public int Sets { get; set; }
}