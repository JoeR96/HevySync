using HevySync.Application.Common;
using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;

namespace HevySync.Application.Workouts.Commands.CompleteWorkoutDay;

public class CompleteWorkoutDayCommandHandler : ICommandHandler<CompleteWorkoutDayCommand, CompleteWorkoutDayResult>
{
    private readonly IWorkoutRepository _workoutRepository;

    public CompleteWorkoutDayCommandHandler(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<CompleteWorkoutDayResult> HandleAsync(
        CompleteWorkoutDayCommand command,
        CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);
        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {command.WorkoutId} not found");
        }

        var completedWeek = workout.Activity.Week;
        var completedDay = workout.Activity.Day;

        // Convert DTOs to domain value objects
        var exercisePerformances = command.ExercisePerformances.Select(ep =>
        {
            var sets = ep.CompletedSets.Select(s => Set.Create(s.WeightKg, s.Reps)).ToList();
            var result = Enum.Parse<PerformanceResult>(ep.PerformanceResult, ignoreCase: true);
            return ExercisePerformance.Create(ep.ExerciseId, sets, result);
        }).ToList();

        // Complete the day
        workout.CompleteDay(exercisePerformances);

        // Save the workout
        await _workoutRepository.UpdateAsync(workout, cancellationToken);

        var weekCompleted = workout.Activity.Day == 1 && completedDay == workout.Activity.WorkoutsInWeek;

        return new CompleteWorkoutDayResult
        {
            WorkoutId = workout.Id,
            CompletedWeek = completedWeek,
            CompletedDay = completedDay,
            NewWeek = workout.Activity.Week,
            NewDay = workout.Activity.Day,
            WeekCompleted = weekCompleted
        };
    }
}

