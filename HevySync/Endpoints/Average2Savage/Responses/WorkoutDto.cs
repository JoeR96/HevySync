namespace HevySync.Endpoints.Average2Savage.Responses;

public record WorkoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<ExerciseDto> Exercises { get; set; }
}