using HevySync.Domain.DomainServices;
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

        var exercisesByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Order).ToList());

        var result = new Dictionary<int, List<SessionExerciseDto>>();

        foreach (var (day, exercises) in exercisesByDay)
        {
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercises)
            {
                var sets = await setGenerationService.GenerateWeekOneSetsAsync(
                    exercise.Progression,
                    workout.Activity,
                    cancellationToken);

                sessionExercises.Add(new SessionExerciseDto(
                    exercise.ExerciseTemplateId,
                    exercise.RestTimer,
                    exercise.Name,
                    sets.Select(s => new SessionSetDto(s.WeightKg, s.Reps)).ToList()));
            }

            result[day] = sessionExercises;
        }

        return result;
    }
}
