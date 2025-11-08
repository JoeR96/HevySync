using HevySync.Application.Common;
using HevySync.Application.DTOs;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;

namespace HevySync.Application.Workouts.Commands.CreateWorkout;

public sealed class CreateWorkoutCommandHandler : ICommandHandler<CreateWorkoutCommand, WorkoutDto>
{
    private readonly IWorkoutRepository _workoutRepository;

    public CreateWorkoutCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<WorkoutDto> HandleAsync(CreateWorkoutCommand command, CancellationToken cancellationToken = default)
    {
        // Create workout name value object
        var workoutName = WorkoutName.Create(command.WorkoutName);

        // Create exercises
        var exercises = new List<Exercise>();
        var workoutId = Guid.NewGuid(); // Temporary ID for exercise creation

        foreach (var exerciseDto in command.Exercises)
        {
            var exerciseName = ExerciseName.Create(exerciseDto.ExerciseName);
            var restTimer = RestTimer.Create(exerciseDto.RestTimer);

            // Parse enums
            BodyCategory? bodyCategory = null;
            if (!string.IsNullOrEmpty(exerciseDto.BodyCategory) && 
                Enum.TryParse<BodyCategory>(exerciseDto.BodyCategory, out var bc))
            {
                bodyCategory = bc;
            }

            EquipmentType? equipmentType = null;
            if (!string.IsNullOrEmpty(exerciseDto.EquipmentType) && 
                Enum.TryParse<EquipmentType>(exerciseDto.EquipmentType, out var et))
            {
                equipmentType = et;
            }

            // Create progression strategy
            ExerciseProgression progression = exerciseDto.Progression switch
            {
                CreateLinearProgressionDto lp => LinearProgressionStrategy.Create(
                    Guid.Empty, // Will be set by Exercise.Create
                    TrainingMax.Create(lp.TrainingMax),
                    WeightProgression.Create(lp.WeightProgression),
                    lp.AttemptsBeforeDeload,
                    lp.IsPrimary),
                CreateRepsPerSetDto rps => RepsPerSetStrategy.Create(
                    Guid.Empty, // Will be set by Exercise.Create
                    RepRange.Create(rps.MinimumReps, rps.TargetReps, rps.MaximumReps),
                    rps.StartingSetCount,
                    rps.TargetSetCount,
                    rps.StartingWeight),
                _ => throw new InvalidOperationException($"Unknown progression type: {exerciseDto.Progression.GetType().Name}")
            };

            var exercise = Exercise.Create(
                exerciseName,
                exerciseDto.ExerciseTemplateId,
                restTimer,
                exerciseDto.Day,
                exerciseDto.Order,
                exerciseDto.NumberOfSets,
                workoutId,
                progression,
                bodyCategory,
                equipmentType);

            exercises.Add(exercise);
        }

        // Create workout aggregate
        var workout = Workout.Create(
            workoutName,
            command.UserId,
            command.WorkoutDaysInWeek,
            exercises);

        // Persist
        await _workoutRepository.AddAsync(workout, cancellationToken);
        await _workoutRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return MapToDto(workout);
    }

    private static WorkoutDto MapToDto(Workout workout)
    {
        return new WorkoutDto
        {
            Id = workout.Id,
            Name = workout.Name,
            UserId = workout.UserId,
            Activity = new WorkoutActivityDto
            {
                Week = workout.Activity.Week,
                Day = workout.Activity.Day,
                WorkoutsInWeek = workout.Activity.WorkoutsInWeek
            },
            Exercises = workout.Exercises.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                ExerciseTemplateId = e.ExerciseTemplateId,
                RestTimer = e.RestTimer,
                Day = e.Day,
                Order = e.Order,
                NumberOfSets = e.NumberOfSets,
                BodyCategory = e.BodyCategory?.ToString(),
                EquipmentType = e.EquipmentType?.ToString(),
                Progression = e.Progression switch
                {
                    LinearProgressionStrategy lp => new LinearProgressionDto
                    {
                        Id = lp.Id,
                        ProgramType = lp.ProgramType.ToString(),
                        TrainingMax = lp.TrainingMax,
                        WeightProgression = lp.WeightProgression,
                        AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                        IsPrimary = lp.IsPrimary
                    },
                    RepsPerSetStrategy rps => new RepsPerSetDto
                    {
                        Id = rps.Id,
                        ProgramType = rps.ProgramType.ToString(),
                        MinimumReps = rps.RepRange.MinimumReps,
                        TargetReps = rps.RepRange.TargetReps,
                        MaximumReps = rps.RepRange.MaximumReps,
                        StartingSetCount = rps.StartingSetCount,
                        TargetSetCount = rps.TargetSetCount,
                        StartingWeight = rps.StartingWeight
                    },
                    _ => throw new InvalidOperationException($"Unknown progression type: {e.Progression.GetType().Name}")
                }
            }).ToList()
        };
    }
}

