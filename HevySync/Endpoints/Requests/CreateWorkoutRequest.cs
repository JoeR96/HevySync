using HevySync.Handlers;

namespace HevySync.Endpoints.Requests;

public record CreateWorkoutRequest
{
    public string WorkoutName { get; set; }
    public List<ExerciseRequest> Exercises { get; set; }
}