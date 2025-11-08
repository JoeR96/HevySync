using HevySync.Application.Common;
using HevySync.Domain.DomainServices;
using HevySync.Domain.Repositories;

namespace HevySync.Application.Workouts.Commands.GenerateWeekOne;

public class GenerateWeekOneCommandHandler : ICommandHandler<GenerateWeekOneCommand, Dictionary<int, List<SessionExerciseDto>>>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISetGenerationService _setGenerationService;

    public GenerateWeekOneCommandHandler(
        IWorkoutRepository workoutRepository,
        ISetGenerationService setGenerationService)
    {
        _workoutRepository = workoutRepository;
        _setGenerationService = setGenerationService;
    }

    public async Task<Dictionary<int, List<SessionExerciseDto>>> HandleAsync(
        GenerateWeekOneCommand command,
        CancellationToken cancellationToken = default)
    {
        // Get workout with exercises
        var workout = await _workoutRepository.GetByIdWithExercisesAsync(command.WorkoutId, cancellationToken);
        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");
        }

        // Group exercises by day
        var exercisesByDay = workout.Exercises
            .GroupBy(e => e.Day)
            .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Order).ToList());

        // Generate sets for each day
        var result = new Dictionary<int, List<SessionExerciseDto>>();

        foreach (var (day, exercises) in exercisesByDay)
        {
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercises)
            {
                // Generate sets using domain service
                var sets = await _setGenerationService.GenerateWeekOneSetsAsync(
                    exercise.Progression,
                    workout.Activity,
                    cancellationToken);

                sessionExercises.Add(new SessionExerciseDto
                {
                    ExerciseTemplateId = exercise.ExerciseTemplateId,
                    RestSeconds = exercise.RestTimer,
                    Notes = exercise.Name,
                    Sets = sets.Select(s => new SessionSetDto
                    {
                        WeightKg = s.WeightKg,
                        Reps = s.Reps
                    }).ToList()
                });
            }

            result[day] = sessionExercises;
        }

        return result;
    }
}

