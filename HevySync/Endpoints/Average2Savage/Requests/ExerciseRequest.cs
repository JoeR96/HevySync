namespace HevySync.Endpoints.Average2Savage.Requests;

public record ExerciseRequest
{
    public int RestTimer { get; set; }
    public int Day { get; set; }
    public int Order { get; set; }
    public string ExerciseName { get; set; }

    public string ExerciseTemplateId { get; set; }
    public ExerciseDetailsRequest ExerciseDetailsRequest { get; set; }
}