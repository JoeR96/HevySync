using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutWeekSessions;

public sealed record GetWorkoutWeekSessionsQuery(Guid WorkoutId) : IRequest<Dictionary<int, List<WorkoutSessionDto>>>;
