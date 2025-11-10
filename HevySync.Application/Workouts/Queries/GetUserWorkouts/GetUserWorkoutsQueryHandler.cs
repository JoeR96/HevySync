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

        var workoutDtos = new List<WorkoutDto>();

        foreach (var workout in workouts)
        {
            var activity = activities.FirstOrDefault(a => a.WorkoutId == workout.Id);
            var currentWeek = workout.Activity.Week;

            // Get all planned sets for the current week
            var weeklyPlans = await unitOfWork.WeeklyExercisePlans
                .GetPlansForWeekAsync(workout.Id, currentWeek, cancellationToken);

            var plansByExerciseId = weeklyPlans.ToDictionary(p => p.ExerciseId);

            var workoutDto = new WorkoutDto
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
                Exercises = workout.Exercises.Select(e =>
                {
                    // Get planned sets for this exercise
                    plansByExerciseId.TryGetValue(e.Id, out var plan);
                    var plannedSets = plan?.PlannedSets
                        .Select(s => new SetDto { WeightKg = s.WeightKg, Reps = s.Reps })
                        .ToList();

                    return new ExerciseDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        ExerciseTemplateId = e.ExerciseTemplateId,
                        RestTimer = e.RestTimer,
                        Day = e.Day,
                        Order = e.Order,
                        NumberOfSets = e.NumberOfSets,
                        PlannedSets = plannedSets,
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
                    };
                }).ToList()
            };

            workoutDtos.Add(workoutDto);
        }

        return workoutDtos;
    }
}
