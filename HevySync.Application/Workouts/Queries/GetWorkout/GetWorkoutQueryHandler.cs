using HevySync.Application.DTOs;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetWorkout;

public sealed class GetWorkoutQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetWorkoutQuery, WorkoutDto?>
{
    public async Task<WorkoutDto?> Handle(GetWorkoutQuery query, CancellationToken cancellationToken)
    {
        var workout = await unitOfWork.Workouts.GetByIdAsync(query.WorkoutId, cancellationToken);

        if (workout == null)
            return null;

        return new WorkoutDto
        {
            Id = workout.Id,
            Name = workout.Name,
            UserId = workout.UserId,
            Activity = new WorkoutActivityDto
            {
                Week = workout.Activity.Week,
                Day = workout.Activity.Day,
                WorkoutsInWeek = workout.Activity.WorkoutsInWeek
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
    }
}
