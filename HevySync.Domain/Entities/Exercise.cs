using HevySync.Domain.Common;
using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;

namespace HevySync.Domain.Entities;

/// <summary>
/// Represents an exercise within a workout.
/// An exercise is an entity within the Workout aggregate.
/// </summary>
public sealed class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; private set; } = null!;
    public string ExerciseTemplateId { get; private set; } = null!;
    public RestTimer RestTimer { get; private set; } = null!;
    public int Day { get; private set; }
    public int Order { get; private set; }
    public int NumberOfSets { get; private set; }
    public BodyCategory? BodyCategory { get; private set; }
    public EquipmentType? EquipmentType { get; private set; }
    public Guid WorkoutId { get; private set; }
    
    // Navigation property for progression strategy
    public ExerciseProgression Progression { get; private set; } = null!;

    // EF Core constructor
    private Exercise()
    {
    }

    private Exercise(
        Guid id,
        ExerciseName name,
        string exerciseTemplateId,
        RestTimer restTimer,
        int day,
        int order,
        int numberOfSets,
        Guid workoutId,
        ExerciseProgression progression,
        BodyCategory? bodyCategory = null,
        EquipmentType? equipmentType = null) : base(id)
    {
        Name = name;
        ExerciseTemplateId = exerciseTemplateId;
        RestTimer = restTimer;
        Day = day;
        Order = order;
        NumberOfSets = numberOfSets;
        WorkoutId = workoutId;
        Progression = progression;
        BodyCategory = bodyCategory;
        EquipmentType = equipmentType;
    }

    public static Exercise Create(
        ExerciseName name,
        string exerciseTemplateId,
        RestTimer restTimer,
        int day,
        int order,
        int numberOfSets,
        Guid workoutId,
        ExerciseProgression progression,
        BodyCategory? bodyCategory = null,
        EquipmentType? equipmentType = null)
    {
        if (string.IsNullOrWhiteSpace(exerciseTemplateId))
        {
            throw new ArgumentException("Exercise template ID cannot be empty", nameof(exerciseTemplateId));
        }

        if (day < 1)
        {
            throw new ArgumentException("Day must be at least 1", nameof(day));
        }

        if (order < 0)
        {
            throw new ArgumentException("Order cannot be negative", nameof(order));
        }

        if (numberOfSets < 1)
        {
            throw new ArgumentException("Number of sets must be at least 1", nameof(numberOfSets));
        }

        var exerciseId = Guid.NewGuid();

        // Set the exercise ID on the progression
        ExerciseProgression progressionWithExerciseId = progression switch
        {
            LinearProgressionStrategy lp => LinearProgressionStrategy.Create(
                exerciseId,
                lp.TrainingMax,
                lp.WeightProgression,
                lp.AttemptsBeforeDeload,
                lp.IsPrimary),
            RepsPerSetStrategy rps => RepsPerSetStrategy.Create(
                exerciseId,
                rps.RepRange,
                rps.StartingSetCount,
                rps.TargetSetCount,
                rps.StartingWeight),
            _ => throw new InvalidExerciseException($"Unsupported progression type: {progression.GetType().Name}")
        };

        return new Exercise(
            exerciseId,
            name,
            exerciseTemplateId,
            restTimer,
            day,
            order,
            numberOfSets,
            workoutId,
            progressionWithExerciseId,
            bodyCategory,
            equipmentType);
    }
}

public class InvalidExerciseException : DomainException
{
    public InvalidExerciseException(string message) : base(message)
    {
    }
}

