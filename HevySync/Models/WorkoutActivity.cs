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
    public Workout Workout { get; set; }
}