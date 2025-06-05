namespace HevySync.Endpoints.Average2Savage.Requests;

public record CreateWorkoutRequest
{
    public string WorkoutName { get; set; }
    public List<ExerciseRequest> Exercises { get; set; }
}