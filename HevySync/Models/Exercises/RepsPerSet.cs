namespace HevySync.Models.Exercises;

public class RepsPerSet : ExerciseDetail
{
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int StartingSetCount { get; set; }
    public int TargetSetCount { get; set; }
    public decimal StartingWeight { get; set; }
}