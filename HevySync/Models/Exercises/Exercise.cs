using HevySync.Handlers;

namespace HevySync.Models.Exercises;

public class Exercise
{
    public Guid Id { get; set; }
    public string ExerciseName { get; set; }
    public int Day { get; set; }
    public WorkoutType Method { get; set; }
    public Category Category { get; set; }
    public EquipmentType EquipmentType { get; set; }

    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; }

    public RepsPerSet? RepsPerSet { get; set; }
    public LinearProgression? LinearProgression { get; set; }
}