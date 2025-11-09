using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetUserWorkouts;

public sealed record GetUserWorkoutsQuery(Guid UserId) : IRequest<List<WorkoutDto>>;
