using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using MediatR;

namespace HevySync.Application.Workouts.Commands.GenerateNextWeek;

public sealed record GenerateNextWeekCommand(
    Guid WorkoutId,
    List<ExercisePerformanceDto> WeekPerformances) : IRequest<Dictionary<int, List<SessionExerciseDto>>>;

public sealed record ExercisePerformanceDto(
    Guid ExerciseId,
    List<CompletedSetDto> CompletedSets,
    string PerformanceResult);

public sealed record CompletedSetDto(
    decimal WeightKg,
    int Reps);
