using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutHistory;

public record GetWorkoutHistoryQuery(Guid WorkoutId) : IRequest<List<WorkoutSessionDto>>;
