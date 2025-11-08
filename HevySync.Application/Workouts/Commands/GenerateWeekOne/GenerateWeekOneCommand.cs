using MediatR;

namespace HevySync.Application.Workouts.Commands.GenerateWeekOne;

public sealed record GenerateWeekOneCommand(Guid WorkoutId) : IRequest<Dictionary<int, List<SessionExerciseDto>>>;

public sealed record SessionExerciseDto(
    string ExerciseTemplateId,
    int RestSeconds,
    string Notes,
    List<SessionSetDto> Sets);

public sealed record SessionSetDto(
    decimal WeightKg,
    int Reps);
