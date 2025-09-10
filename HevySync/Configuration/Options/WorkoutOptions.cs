namespace HevySync.Configuration.Options;

public class WorkoutOptions
{
    public const string SectionName = "Workout";

    public int DefaultDecimalPlaces { get; set; } = 2;
    public int DefaultWeek { get; set; } = 1;
    public int DefaultDay { get; set; } = 1;
}