using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkoutSession;

public record GetWorkoutSessionQuery(Guid WorkoutId, int Week, int Day) : IRequest<WorkoutSessionDto?>;
