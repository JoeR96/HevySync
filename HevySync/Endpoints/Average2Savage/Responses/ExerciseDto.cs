using HevySync.Models.Exercises;

namespace HevySync.Endpoints.Average2Savage.Responses;

public class ExerciseResponseDto
{
    public Guid Id { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Day { get; set; }
    public int Order { get; set; }
    public int RestTimer { get; set; }
    public int NumberOfSets { get; set; }
    public ExerciseDetailDto ExerciseDetail { get; set; } = null!;
    public List<SetResponseDto>? Sets { get; set; }
}

public class SetResponseDto
{
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
}
