using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;
using MediatR;

namespace HevySync.Application.Workouts.Commands.CompleteWorkoutDay;

public sealed class CompleteWorkoutDayCommandHandler(
    IUnitOfWork unitOfWork,
    ISetGenerationService setGenerationService)
    : IRequestHandler<CompleteWorkoutDayCommand, CompleteWorkoutDayResult>
{
    public async Task<CompleteWorkoutDayResult> Handle(
        CompleteWorkoutDayCommand command,
        CancellationToken cancellationToken)
    {
        var workout = await unitOfWork.Workouts.GetByIdAsync(command.WorkoutId, cancellationToken);

        if (workout == null)
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");

        var completedWeek = workout.Activity.Week;
        var completedDay = workout.Activity.Day;

        var exercisePerformances = command.ExercisePerformances.Select(ep =>
        {
            var sets = ep.CompletedSets.Select(s => Set.Create(s.WeightKg, s.Reps)).ToList();
            var result = Enum.Parse<PerformanceResult>(ep.PerformanceResult, ignoreCase: true);
            return ExercisePerformance.Create(ep.ExerciseId, sets, result);
        }).ToList();

        // Create a workout session to store the historical performance data
        var workoutSession = WorkoutSession.Create(
            workout.Id,
            completedWeek,
            completedDay,
            DateTimeOffset.UtcNow,
            exercisePerformances);

        await unitOfWork.WorkoutSessions.AddAsync(workoutSession, cancellationToken);

        workout.CompleteDay(exercisePerformances);

        unitOfWork.Workouts.Update(workout);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate next week's plan for this day's exercises
        // This ensures that by the time we complete all days in the current week,
        // the entire next week is ready
        var nextWeek = completedWeek + 1;
        var exercisesForDay = workout.GetExercisesForDay(completedDay);

        foreach (var exercise in exercisesForDay)
        {
            // Check if plan already exists for this exercise in next week
            var existingPlan = await unitOfWork.WeeklyExercisePlans
                .GetPlanForExerciseAsync(exercise.Id, nextWeek, cancellationToken);

            if (existingPlan == null)
            {
                // Generate sets for next week based on current progression state
                // The progression will be applied at the end of the current week
                var sets = await setGenerationService.GenerateWeekOneSetsAsync(
                    exercise.Progression,
                    workout.Activity,
                    cancellationToken);

                var plan = WeeklyExercisePlan.Create(
                    workout.Id,
                    exercise.Id,
                    nextWeek,
                    sets.ToList());

                await unitOfWork.WeeklyExercisePlans.AddAsync(plan, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var weekCompleted = workout.Activity.Day == 1 && completedDay == workout.Activity.WorkoutsInWeek;

        return new CompleteWorkoutDayResult(
            workout.Id,
            completedWeek,
            completedDay,
            workout.Activity.Week,
            workout.Activity.Day,
            weekCompleted);
    }
}
