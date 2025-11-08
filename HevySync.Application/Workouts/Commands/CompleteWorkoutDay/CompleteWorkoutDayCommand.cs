using MediatR;

namespace HevySync.Application.Workouts.Commands.CompleteWorkoutDay;

public sealed record CompleteWorkoutDayCommand(
    Guid WorkoutId,
    List<ExercisePerformanceDto> ExercisePerformances) : IRequest<CompleteWorkoutDayResult>;

public sealed record ExercisePerformanceDto(
    Guid ExerciseId,
    List<CompletedSetDto> CompletedSets,
    string PerformanceResult);

public sealed record CompletedSetDto(
    decimal WeightKg,
    int Reps);

public sealed record CompleteWorkoutDayResult(
    Guid WorkoutId,
    int CompletedWeek,
    int CompletedDay,
    int NewWeek,
    int NewDay,
    bool WeekCompleted);
