using HevySync.Application.Common;
using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Domain.Aggregates;
using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;

namespace HevySync.Application.Workouts.Commands.GenerateNextWeek;

/// <summary>
/// Handler for generating the next week of a workout program.
/// Applies progression based on performance and generates new sets.
/// </summary>
public sealed class GenerateNextWeekCommandHandler : ICommandHandler<GenerateNextWeekCommand, Dictionary<int, List<SessionExerciseDto>>>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly ISetGenerationService _setGenerationService;

    public GenerateNextWeekCommandHandler(
        IWorkoutRepository workoutRepository,
        ISetGenerationService setGenerationService)
    {
        _workoutRepository = workoutRepository;
        _setGenerationService = setGenerationService;
    }

    public async Task<Dictionary<int, List<SessionExerciseDto>>> HandleAsync(
        GenerateNextWeekCommand command,
        CancellationToken cancellationToken = default)
    {
        // Get the workout with exercises
        var workout = await _workoutRepository.GetByIdWithExercisesAsync(command.WorkoutId, cancellationToken);

        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");
        }

        // Convert DTOs to domain value objects
        var weekPerformances = command.WeekPerformances.Select(p =>
        {
            var completedSets = p.CompletedSets.Select(s => Set.Create(s.WeightKg, s.Reps)).ToList();
            var result = Enum.Parse<PerformanceResult>(p.PerformanceResult, ignoreCase: true);
            return ExercisePerformance.Create(p.ExerciseId, completedSets, result);
        }).ToList();

        // Apply progression to exercises based on performance
        workout.ApplyProgression(weekPerformances);

        // Save the updated workout
        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        // Generate sets for all days in the week
        var weekSessions = new Dictionary<int, List<SessionExerciseDto>>();

        for (int day = 1; day <= workout.Activity.WorkoutsInWeek; day++)
        {
            var exercisesForDay = workout.GetExercisesForDay(day);
            var sessionExercises = new List<SessionExerciseDto>();

            foreach (var exercise in exercisesForDay)
            {
                var sets = await _setGenerationService.GenerateWeekOneSetsAsync(
                    exercise.Progression,
                    workout.Activity,
                    cancellationToken);

                sessionExercises.Add(new SessionExerciseDto
                {
                    ExerciseTemplateId = exercise.ExerciseTemplateId,
                    RestSeconds = exercise.RestTimer.Seconds,
                    Notes = string.Empty,
                    Sets = sets.Select(s => new SessionSetDto
                    {
                        WeightKg = s.WeightKg,
                        Reps = s.Reps
                    }).ToList()
                });
            }

            weekSessions[day] = sessionExercises;
        }

        return weekSessions;
    }
}

