namespace HevySync.Models.Exercises;

public class LinearProgression : ExerciseDetail
{
    public decimal TrainingMax { get; set; }
    public decimal WeightProgression { get; set; }
    public int AttemptsBeforeDeload { get; set; }
    public bool Primary { get; set; }
}