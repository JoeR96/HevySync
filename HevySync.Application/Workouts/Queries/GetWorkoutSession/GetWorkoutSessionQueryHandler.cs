using HevySync.Application.DTOs;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutSession;

public sealed class GetWorkoutSessionQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetWorkoutSessionQuery, WorkoutSessionDto?>
{
    public async Task<WorkoutSessionDto?> Handle(GetWorkoutSessionQuery query, CancellationToken cancellationToken)
    {
        var sessions = await unitOfWork.WorkoutSessions
            .FindAsync(
                ws => ws.WorkoutId == query.WorkoutId && ws.Week == query.Week && ws.Day == query.Day,
                cancellationToken);

        var session = sessions.FirstOrDefault();
        if (session == null)
            return null;

        // Get the workout to access exercise information
        var workout = await unitOfWork.Workouts.GetByIdAsync(session.WorkoutId, cancellationToken);
        if (workout == null)
            return null;

        var exerciseMap = workout.Exercises.ToDictionary(e => e.Id);

        return new WorkoutSessionDto
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
        };
    }
}
