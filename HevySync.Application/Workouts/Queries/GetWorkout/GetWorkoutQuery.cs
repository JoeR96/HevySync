using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkout;

public sealed record GetWorkoutQuery(Guid WorkoutId) : IRequest<WorkoutDto?>;

