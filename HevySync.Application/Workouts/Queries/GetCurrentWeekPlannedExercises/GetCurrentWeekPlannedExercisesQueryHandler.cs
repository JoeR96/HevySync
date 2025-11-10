using HevySync.Application.DTOs;
using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Repositories;
using MediatR;

namespace HevySync.Application.Workouts.Queries.GetCurrentWeekPlannedExercises;

/// <summary>
/// Query handler for the Workout Planning Context.
/// Returns all exercises for the current week with:
/// - Completed session data (actual sets performed) for past days
/// - Planned sets for current and future days
/// This provides a complete week overview combining history and planning.
/// </summary>
public sealed class GetCurrentWeekPlannedExercisesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCurrentWeekPlannedExercisesQuery, CurrentWeekPlannedExercisesDto?>
{
    public async Task<CurrentWeekPlannedExercisesDto?> Handle(
        GetCurrentWeekPlannedExercisesQuery query,
        CancellationToken cancellationToken)
    {
        // Find the user's active workout
        var activities = await unitOfWork.Activities.FindAsync(
            a => a.UserId == query.UserId && a.Status == ActivityStatus.Active,
            cancellationToken);

        var activeActivity = activities.FirstOrDefault();
        if (activeActivity == null)
            return null;

        // Get the workout with all exercises
        var workout = await unitOfWork.Workouts.GetByIdAsync(activeActivity.WorkoutId, cancellationToken);
        if (workout == null)
            return null;

        var currentWeek = workout.Activity.Week;
        var currentDay = workout.Activity.Day;

        // Get all planned sets for the current week
        var weeklyPlans = await unitOfWork.WeeklyExercisePlans
            .GetPlansForWeekAsync(activeActivity.WorkoutId, currentWeek, cancellationToken);

        var plansByExerciseId = weeklyPlans.ToDictionary(p => p.ExerciseId);

        // Get all completed sessions for the current week
        var completedSessions = await unitOfWork.WorkoutSessions
            .FindWithExercisesAsync(
                ws => ws.WorkoutId == activeActivity.WorkoutId && ws.Week == currentWeek,
                cancellationToken);

        // Create a lookup: Day -> ExerciseId -> Completed Performance
        var completedByDayAndExercise = completedSessions
            .GroupBy(s => s.Day)
            .ToDictionary(
                g => g.Key,
                g => g.SelectMany(s => s.ExercisePerformances)
                    .ToDictionary(ep => ep.ExerciseId)
            );

        // Build the response - exercises grouped by day with their sets (completed or planned)
        var exercisesByDay = new Dictionary<int, List<PlannedExerciseDto>>();

        foreach (var dayGroup in workout.Exercises.GroupBy(e => e.Day).OrderBy(g => g.Key))
        {
            var dayExercises = new List<PlannedExerciseDto>();

            foreach (var e in dayGroup.OrderBy(ex => ex.Order))
            {
                var day = e.Day;
                List<SetDto> sets;
                bool isCompleted = false;

                // Check if this day has been completed
                if (completedByDayAndExercise.TryGetValue(day, out var exercisesForDay) &&
                    exercisesForDay.TryGetValue(e.Id, out var completedPerformance))
                {
                    // Use completed sets for past days
                    sets = completedPerformance.CompletedSets
                        .Select(s => new SetDto { WeightKg = s.WeightKg, Reps = s.Reps })
                        .ToList();
                    isCompleted = true;
                }
                else
                {
                    // Use planned sets for current/future days
                    // All weekly plans should be pre-generated
                    plansByExerciseId.TryGetValue(e.Id, out var plan);
                    sets = plan?.PlannedSets
                        .Select(s => new SetDto { WeightKg = s.WeightKg, Reps = s.Reps })
                        .ToList() ?? new List<SetDto>();
                    isCompleted = false;
                }

                dayExercises.Add(new PlannedExerciseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    RestTimer = e.RestTimer,
                    Day = e.Day,
                    Order = e.Order,
                    NumberOfSets = e.NumberOfSets,
                    PlannedSets = sets,
                    IsCompleted = isCompleted,
                    Progression = e.Progression switch
                    {
                        LinearProgressionStrategy lp => new LinearProgressionDto
                        {
                            Id = lp.Id,
                            ProgramType = lp.ProgramType.ToString(),
                            TrainingMax = lp.TrainingMax.Value,
                            WeightProgression = lp.WeightProgression.Value,
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
                            WeightProgression = rps.WeightProgression.Value
                        },
                        _ => throw new InvalidOperationException($"Unknown progression type: {e.Progression.GetType().Name}")
                    }
                });
            }

            exercisesByDay[dayGroup.Key] = dayExercises;
        }

        return new CurrentWeekPlannedExercisesDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name.Value,
            Week = currentWeek,
            CurrentDay = currentDay,
            TotalDaysInWeek = workout.Activity.WorkoutsInWeek,
            ExercisesByDay = exercisesByDay
        };
    }
}
