using HevySync.Models;
using HevySync.Models.Exercises;

namespace HevySync.Endpoints.Average2Savage.Responses;

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