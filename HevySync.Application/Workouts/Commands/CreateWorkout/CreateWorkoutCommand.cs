using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Commands.CreateWorkout;

public sealed record CreateWorkoutCommand(
    string WorkoutName,
    Guid UserId,
    int WorkoutDaysInWeek,
    List<CreateExerciseDto> Exercises) : IRequest<WorkoutDto>;

public sealed record CreateExerciseDto(
    string ExerciseName,
    string ExerciseTemplateId,
    int RestTimer,
    int Day,
    int Order,
    int NumberOfSets,
    CreateProgressionDto Progression);

public abstract record CreateProgressionDto(string ProgramType);

public sealed record CreateLinearProgressionDto(
    decimal TrainingMax,
    decimal WeightProgression,
    int AttemptsBeforeDeload,
    bool IsPrimary) : CreateProgressionDto("LinearProgression");

public sealed record CreateRepsPerSetDto(
    int MinimumReps,
    int TargetReps,
    int MaximumReps,
    int StartingSetCount,
    int TargetSetCount,
    decimal StartingWeight,
    decimal WeightProgression) : CreateProgressionDto("RepsPerSet");

