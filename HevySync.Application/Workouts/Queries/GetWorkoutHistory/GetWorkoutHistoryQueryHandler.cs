using HevySync.Application.DTOs;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutHistory;

public sealed class GetWorkoutHistoryQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkoutHistoryQuery, List<WorkoutSessionDto>>
{
    public async Task<List<WorkoutSessionDto>> Handle(GetWorkoutHistoryQuery query, CancellationToken cancellationToken)
    {
        var sessions = await unitOfWork.WorkoutSessions
            .FindAsync(ws => ws.WorkoutId == query.WorkoutId, cancellationToken);

        // Get the workout to access exercise information
        var workout = await unitOfWork.Workouts.GetByIdAsync(query.WorkoutId, cancellationToken);
        if (workout == null)
            return new List<WorkoutSessionDto>();

        var exerciseMap = workout.Exercises.ToDictionary(e => e.Id);

        return sessions
            .OrderByDescending(s => s.Week)
            .ThenByDescending(s => s.Day)
            .Select(session => new WorkoutSessionDto
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
            })
            .ToList();
    }
}
