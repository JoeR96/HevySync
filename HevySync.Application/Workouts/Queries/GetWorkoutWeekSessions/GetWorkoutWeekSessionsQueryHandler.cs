using HevySync.Application.DTOs;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutWeekSessions;

public sealed class GetWorkoutWeekSessionsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkoutWeekSessionsQuery, Dictionary<int, List<WorkoutSessionDto>>>
{
    public async Task<Dictionary<int, List<WorkoutSessionDto>>> Handle(
        GetWorkoutWeekSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var sessions = await unitOfWork.WorkoutSessions
            .FindWithExercisesAsync(ws => ws.WorkoutId == query.WorkoutId, cancellationToken);

        // Get the workout to access exercise information
        var workout = await unitOfWork.Workouts.GetByIdAsync(query.WorkoutId, cancellationToken);
        if (workout == null)
            return new Dictionary<int, List<WorkoutSessionDto>>();

        var exerciseMap = workout.Exercises.ToDictionary(e => e.Id);

        // Group sessions by week
        return sessions
            .OrderBy(s => s.Week)
            .ThenBy(s => s.Day)
            .GroupBy(s => s.Week)
            .ToDictionary(
                g => g.Key,
                g => g.Select(session => new WorkoutSessionDto
                {
                    Id = session.Id,
                    WorkoutId = session.WorkoutId,
                    Week = session.Week,
                    Day = session.Day,
                    CompletedAt = session.CompletedAt,
                    ExercisePerformances = session.ExercisePerformances.Select(ep =>
                    {
                        var exercise = exerciseMap.GetValueOrDefault(ep.ExerciseId);
                        return new SessionExercisePerformanceDto
                        {
                            Id = ep.Id,
                            ExerciseId = ep.ExerciseId,
                            ExerciseName = exercise?.Name ?? "Unknown",
                            ExerciseTemplateId = exercise?.ExerciseTemplateId ?? string.Empty,
                            PerformanceResult = ep.Result.ToString(),
                            CompletedSets = ep.CompletedSets.Select(s => new SetDto
                            {
                                WeightKg = s.WeightKg,
                                Reps = s.Reps
                            }).ToList()
                        };
                    }).ToList()
                }).ToList()
            );
    }
}
