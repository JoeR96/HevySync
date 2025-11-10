using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetCurrentCycleWeekSessions;

public sealed record GetCurrentCycleWeekSessionsQuery(Guid UserId) : IRequest<Dictionary<int, List<WorkoutSessionDto>>?>;
