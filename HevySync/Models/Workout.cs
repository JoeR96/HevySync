using System.ComponentModel.DataAnnotations.Schema;
using HevySync.Models.Exercises;

namespace HevySync.Models;

public class Workout
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public Guid ApplicationUserId { get; set; }
    public ICollection<Exercise> Exercises { get; set; }
    public WorkoutActivity WorkoutActivity { get; set; }
}

public record WorkoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<ExerciseDto> Exercises { get; set; }
    public WorkoutActivityDto WorkoutActivity { get; set; }
}

public static class WorkoutMappingExtensions
{
    public static WorkoutDto ToDto(this Workout workout)
    {
        return new WorkoutDto
        {
            Id = workout.Id,
            Name = workout.Name,
            WorkoutActivity = workout.WorkoutActivity.ToDto(workout.Id),
            Exercises = workout.Exercises.Select(e => e.ToDto()).ToList()
        };
    }
}