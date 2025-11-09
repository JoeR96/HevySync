using System.ComponentModel.DataAnnotations.Schema;

namespace HevySync.Models;

public class WorkoutActivity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public int Week { get; set; } = 1;
    public int Day { get; set; } = 1;
    public int WorkoutsInWeek { get; set; }
    public Guid WorkoutId { get; set; }
}

public record WorkoutActivityDto
{
    public Guid Id { get; set; }
    public int Week { get; set; }
    public int Day { get; set; }
    public int WorkoutsInWeek { get; set; }
    public Guid WorkoutId { get; set; }
    public string? Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public static class WorkoutActivityMappingExtensions
{
    public static WorkoutActivityDto ToDto(this WorkoutActivity workoutActivity, Guid workoutId)
    {
        return new WorkoutActivityDto
        {
            Week = workoutActivity.Week,
            Day = workoutActivity.Day,
            Id = workoutActivity.Id,
            WorkoutId = workoutId,
            WorkoutsInWeek = workoutActivity.WorkoutsInWeek
        };
    }
}