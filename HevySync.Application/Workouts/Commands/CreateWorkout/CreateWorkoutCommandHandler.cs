using HevySync.Application.DTOs;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;
using MediatR;

namespace HevySync.Application.Workouts.Commands.CreateWorkout;

public sealed class CreateWorkoutCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateWorkoutCommand, WorkoutDto>
{
    public async Task<WorkoutDto> Handle(CreateWorkoutCommand command, CancellationToken cancellationToken)
    {
        // Stop any existing active activities before creating a new one
        var existingActiveActivities = await unitOfWork.Activities
            .FindAsync(a => a.UserId == command.UserId && a.Status == ActivityStatus.Active, cancellationToken);

        if (existingActiveActivities != null && existingActiveActivities.Count > 0)
        {
            foreach (var existingActivity in existingActiveActivities)
            {
                existingActivity.Stop();
            }
        }

        var workoutName = WorkoutName.Create(command.WorkoutName);
        var exercises = new List<Exercise>();
        var workoutId = Guid.NewGuid();

        foreach (var exerciseDto in command.Exercises)
        {
            var exerciseName = ExerciseName.Create(exerciseDto.ExerciseName);
            var restTimer = RestTimer.Create(exerciseDto.RestTimer);

            ExerciseProgression progression = exerciseDto.Progression switch
            {
                CreateLinearProgressionDto lp => LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(lp.TrainingMax),
                    WeightProgression.Create(lp.WeightProgression),
                    lp.AttemptsBeforeDeload,
                    lp.IsPrimary),
                CreateRepsPerSetDto rps => RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(rps.MinimumReps, rps.TargetReps, rps.MaximumReps),
                    rps.StartingSetCount,
                    rps.TargetSetCount,
                    rps.StartingWeight,
                    WeightProgression.Create(rps.WeightProgression)),
                _ => throw new InvalidOperationException($"Unknown progression type: {exerciseDto.Progression.GetType().Name}")
            };

            var exercise = Exercise.Create(
                exerciseName,
                exerciseDto.ExerciseTemplateId,
                restTimer,
                exerciseDto.Day,
                exerciseDto.Order,
                exerciseDto.NumberOfSets,
                workoutId,
                progression);

            exercises.Add(exercise);
        }

        var workout = Workout.Create(
            workoutName,
            command.UserId,
            command.WorkoutDaysInWeek,
            exercises);

        var activity = Activity.Create(command.UserId, workout.Id, workoutName);

        // Save Activity first to satisfy FK constraint
        await unitOfWork.Activities.AddAsync(activity, cancellationToken);
        await unitOfWork.Workouts.AddAsync(workout, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Return the in-memory workout which has all properties populated
        return MapToDto(workout);
    }

    private static WorkoutDto MapToDto(Workout workout) =>
        new()
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

