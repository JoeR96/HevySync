using HevySync.Application.DTOs;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetCurrentWeekPlannedExercises;

public sealed record GetCurrentWeekPlannedExercisesQuery(Guid UserId)
    : IRequest<CurrentWeekPlannedExercisesDto?>;
