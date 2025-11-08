using HevySync.Domain.Repositories;
using HevySync.Domain.ValueObjects;
using MediatR;

namespace HevySync.Application.Workouts.Commands.CompleteWorkoutDay;

public sealed class CompleteWorkoutDayCommandHandler(IUnitOfWork unitOfWork)
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

        workout.CompleteDay(exercisePerformances);

        unitOfWork.Workouts.Update(workout);
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
