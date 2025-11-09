using HevySync.Application.DTOs;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetUserWorkouts;

public sealed class GetUserWorkoutsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserWorkoutsQuery, List<WorkoutDto>>
{
    public async Task<List<WorkoutDto>> Handle(GetUserWorkoutsQuery query, CancellationToken cancellationToken)
    {
        var workouts = await unitOfWork.Workouts.GetUserWorkoutsWithDetailsAsync(query.UserId, cancellationToken);
        var activities = await unitOfWork.Activities.FindAsync(a => a.UserId == query.UserId, cancellationToken);

        return workouts.Select(workout =>
        {
            var activity = activities.FirstOrDefault(a => a.WorkoutId == workout.Id);

            return new WorkoutDto
            {
                Id = workout.Id,
                Name = workout.Name.Value,
                UserId = workout.UserId,
                Activity = new WorkoutActivityDto
                {
                    Week = workout.Activity.Week,
                    Day = workout.Activity.Day,
                    WorkoutsInWeek = workout.Activity.WorkoutsInWeek,
                    Status = activity?.Status.ToString() ?? "Unknown",
                    StartedAt = activity?.StartedAt,
                    CompletedAt = activity?.CompletedAt
                },
                Exercises = workout.Exercises.Select(e => new ExerciseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    RestTimer = e.RestTimer,
                    Day = e.Day,
                    Order = e.Order,
                    NumberOfSets = e.NumberOfSets,
                    Progression = e.Progression switch
                    {
                        LinearProgressionStrategy lp => new LinearProgressionDto
                        {
                            Id = lp.Id,
                            ProgramType = lp.ProgramType.ToString(),
                            TrainingMax = lp.TrainingMax,
                            WeightProgression = lp.WeightProgression,
                            AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                            IsPrimary = lp.IsPrimary
                        },
                        RepsPerSetStrategy rps => new RepsPerSetDto
                        {
                            Id = rps.Id,
                            ProgramType = rps.ProgramType.ToString(),
                            MinimumReps = rps.RepRange.MinimumReps,
                            TargetReps = rps.RepRange.TargetReps,
                            MaximumReps = rps.RepRange.MaximumReps,
                            StartingSetCount = rps.StartingSetCount,
                            TargetSetCount = rps.TargetSetCount,
                            StartingWeight = rps.StartingWeight,
                            WeightProgression = rps.WeightProgression
                        },
                        _ => throw new InvalidOperationException($"Unknown progression type: {e.Progression.GetType().Name}")
                    }
                }).ToList()
            };
        }).ToList();
    }
}
