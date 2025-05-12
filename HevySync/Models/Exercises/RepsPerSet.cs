namespace HevySync.Models.Exercises;

public class RepsPerSet : ExerciseDetail
{
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int NumberOfSets { get; set; }
    public int TotalNumberOfSets { get; set; }
}