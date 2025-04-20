namespace HevySync.Models.Exercises;

public class RepsPerSet
{
    public Guid Id { get; set; }
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int NumberOfSets { get; set; }
    public int TotalNumberOfSets { get; set; }

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
}