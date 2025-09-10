namespace HevySync.Models;

public class WeeklyWorkoutPlan
{
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; }
    public int Week { get; set; }
    public List<DailyWorkout> DailyWorkouts { get; set; } = new();
}

public record WeeklyWorkoutPlanDto
{
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; }
    public int Week { get; set; }
    public List<DailyWorkoutDto> DailyWorkouts { get; set; } = new();
}

public static class WeeklyWorkoutPlanMapping
{
    public static WeeklyWorkoutPlanDto ToDto(this WeeklyWorkoutPlan weeklyWorkoutPlan)
    {
        return new WeeklyWorkoutPlanDto
        {
            WorkoutId = weeklyWorkoutPlan.WorkoutId,
            WorkoutName = weeklyWorkoutPlan.WorkoutName,
            Week = weeklyWorkoutPlan.Week,
            DailyWorkouts = weeklyWorkoutPlan.DailyWorkouts.Select(dw => dw.ToDto()).ToList()
        };
    }
}