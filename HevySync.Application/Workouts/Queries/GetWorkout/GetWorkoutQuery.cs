using HevySync.Application.Common;
using HevySync.Application.DTOs;

namespace HevySync.Application.Workouts.Queries.GetWorkout;

public sealed record GetWorkoutQuery(Guid WorkoutId) : IQuery<WorkoutDto?>;

