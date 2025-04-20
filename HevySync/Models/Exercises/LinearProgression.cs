namespace HevySync.Models.Exercises;

public class LinearProgression
{
    public Guid Id { get; set; }
    public decimal WeightProgression { get; set; }
    public int AttemptsBeforeDeload { get; set; }

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
}