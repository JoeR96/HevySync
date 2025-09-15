using HevySync.Services;

namespace HevySync.Models;

public class DailyWorkout
{
    public int Day { get; set; }
    public List<SessionExercise> SessionExercises { get; set; } = new();
}

public record DailyWorkoutDto
{
    public int Day { get; set; }
    public List<SessionExerciseDto> SessionExercises { get; set; } = new();
}

public static class DailyWorkoutMappings
{
    public static DailyWorkoutDto ToDto(this IGrouping<int, SessionExercise> dayGroup)
    {
        return new DailyWorkoutDto
        {
            Day = dayGroup.Key,
            SessionExercises = dayGroup.Select(se => se.ToDto()).ToList()
        };
    }

    public static DailyWorkoutDto ToDto(this DailyWorkout dayGroup)
    {
        return new DailyWorkoutDto
        {
            Day = dayGroup.Day,
            SessionExercises = dayGroup.SessionExercises.Select(se => se.ToDto()).ToList()
        };
    }
}