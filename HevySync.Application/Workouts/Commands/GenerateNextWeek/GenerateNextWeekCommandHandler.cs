using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Domain.DomainServices;
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

        for (int day = 1; day <= workout.Activity.WorkoutsInWeek; day++)
        {
            var exercisesForDay = workout.GetExercisesForDay(day);
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercisesForDay)
            {
                var sets = await setGenerationService.GenerateWeekOneSetsAsync(
                    exercise.Progression,
                    workout.Activity,
                    cancellationToken);

                sessionExercises.Add(new SessionExerciseDto(
                    exercise.ExerciseTemplateId,
                    exercise.RestTimer.Seconds,
                    string.Empty,
                    sets.Select(s => new SessionSetDto(s.WeightKg, s.Reps)).ToList()));
            }

            weekSessions[day] = sessionExercises;
        }

        return weekSessions;
    }
}
