using HevySync.Endpoints.Average2Savage.Enums;

namespace HevySync.Models.Exercises;

public class Exercise
{
    public Guid Id { get; set; }
    public string ExerciseName { get; set; }
    public string ExerciseTemplateId { get; set; }
    public int RestTimer { get; set; }
    public int Day { get; set; }
    public ExerciseProgram ExerciseProgram { get; set; }
    public BodyCategory BodyCategory { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; }
    public ExerciseDetail ExerciseDetail { get; set; }
    public int Order { get; set; }
    public int NumberOfSets { get; set; }
}