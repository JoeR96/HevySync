using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Commands.GenerateWeekOne;

public sealed class GenerateWeekOneCommandHandler(
    IUnitOfWork unitOfWork,
    ISetGenerationService setGenerationService) : IRequestHandler<GenerateWeekOneCommand, Dictionary<int, List<SessionExerciseDto>>>
{
    public async Task<Dictionary<int, List<SessionExerciseDto>>> Handle(
        GenerateWeekOneCommand command,
        CancellationToken cancellationToken)
    {
        var workout = await unitOfWork.Workouts.GetByIdAsync(command.WorkoutId, cancellationToken);

        if (workout == null)
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");

        var currentWeek = workout.Activity.Week;

        // Generate and persist planned sets for all exercises
        foreach (var exercise in workout.Exercises)
        {
            var sets = await setGenerationService.GenerateWeekOneSetsAsync(
                exercise.Progression,
                workout.Activity,
                cancellationToken);

            // Check if plan already exists for this exercise and week
            var existingPlan = await unitOfWork.WeeklyExercisePlans
                .GetPlanForExerciseAsync(exercise.Id, currentWeek, cancellationToken);

            if (existingPlan == null)
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
        var exercisesByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Order).ToList());

        var result = new Dictionary<int, List<SessionExerciseDto>>();

        foreach (var (day, exercises) in exercisesByDay)
        {
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercises)
            {
                var plan = await unitOfWork.WeeklyExercisePlans
                    .GetPlanForExerciseAsync(exercise.Id, currentWeek, cancellationToken);

                var sets = plan?.PlannedSets.Select(s => new SessionSetDto(s.WeightKg, s.Reps)).ToList()
                    ?? new List<SessionSetDto>();

                sessionExercises.Add(new SessionExerciseDto(
                    exercise.ExerciseTemplateId,
                    exercise.RestTimer,
                    exercise.Name,
                    sets));
            }

            result[day] = sessionExercises;
        }

        return result;
    }
}
