using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;
using MediatR;

namespace HevySync.Application.Workouts.Commands.GenerateNextWeek;

public sealed class GenerateNextWeekCommandHandler(
    IUnitOfWork unitOfWork,
    ISetGenerationService setGenerationService)
    : IRequestHandler<GenerateNextWeekCommand, Dictionary<int, List<SessionExerciseDto>>>
{
    public async Task<Dictionary<int, List<SessionExerciseDto>>> Handle(
        GenerateNextWeekCommand command,
        CancellationToken cancellationToken)
    {
        var workout = await unitOfWork.Workouts.GetByIdAsync(command.WorkoutId, cancellationToken);

        if (workout == null)
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");

        var weekPerformances = command.WeekPerformances.Select(p =>
        {
            var completedSets = p.CompletedSets.Select(s => Set.Create(s.WeightKg, s.Reps)).ToList();
            var result = Enum.Parse<PerformanceResult>(p.PerformanceResult, ignoreCase: true);
            return ExercisePerformance.Create(p.ExerciseId, completedSets, result);
        }).ToList();

        workout.ApplyProgression(weekPerformances);

        unitOfWork.Workouts.Update(workout);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var weekSessions = new Dictionary<int, List<SessionExerciseDto>>();
        var currentWeek = workout.Activity.Week;

        // Generate and persist planned sets for all exercises in the current week
        foreach (var exercise in workout.Exercises)
        {
            var sets = await setGenerationService.GenerateWeekOneSetsAsync(
                exercise.Progression,
                workout.Activity,
                cancellationToken);

            // Create or update the weekly exercise plan
            var existingPlan = await unitOfWork.WeeklyExercisePlans
                .GetPlanForExerciseAsync(exercise.Id, currentWeek, cancellationToken);

            if (existingPlan != null)
            {
                existingPlan.UpdatePlannedSets(sets.ToList());
                unitOfWork.WeeklyExercisePlans.Update(existingPlan);
            }
            else
            {
                var plan = WeeklyExercisePlan.Create(
                    workout.Id,
                    exercise.Id,
                    currentWeek,
                    sets.ToList());
                await unitOfWork.WeeklyExercisePlans.AddAsync(plan, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Group exercises by day for response
        for (int day = 1; day <= workout.Activity.WorkoutsInWeek; day++)
        {
            var exercisesForDay = workout.GetExercisesForDay(day);
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercisesForDay)
            {
                var plan = await unitOfWork.WeeklyExercisePlans
                    .GetPlanForExerciseAsync(exercise.Id, currentWeek, cancellationToken);

                var sets = plan?.PlannedSets.Select(s => new SessionSetDto(s.WeightKg, s.Reps)).ToList()
                    ?? new List<SessionSetDto>();

                sessionExercises.Add(new SessionExerciseDto(
                    exercise.ExerciseTemplateId,
                    exercise.RestTimer.Seconds,
                    string.Empty,
                    sets));
            }

            weekSessions[day] = sessionExercises;
        }

        return weekSessions;
    }
}
