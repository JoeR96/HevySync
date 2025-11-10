using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;
using HevySync.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedDemoUserAsync(
        HevySyncDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        const string demoEmail = "demo@hevysync.com";
        const string demoPassword = "DemoPass123#";

        var existingUser = await userManager.FindByEmailAsync(demoEmail);

        ApplicationUser demoUser;
        if (existingUser != null)
        {
            demoUser = existingUser;

            // Check if workouts exist but sessions don't - this happens when user existed before migration
            var hasWorkouts = await context.Workouts.AnyAsync(w => w.UserId == demoUser.Id);
            var hasSessions = await context.WorkoutSessions.AnyAsync();
            var hasWeeklyPlans = await context.WeeklyExercisePlans.AnyAsync();

            if (hasWorkouts && !hasSessions)
            {
                // Re-seed the workout sessions for existing workouts
                await SeedWorkoutSessionsForExistingWorkouts(context, demoUser.Id);
                await context.SaveChangesAsync();
            }

            if (hasWorkouts && !hasWeeklyPlans)
            {
                // Re-seed the weekly exercise plans for existing workouts
                await SeedWeeklyExercisePlansForExistingWorkouts(context, demoUser.Id);
                await context.SaveChangesAsync();
            }
            return;
        }

        demoUser = new ApplicationUser
        {
            UserName = demoEmail,
            Email = demoEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(demoUser, demoPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create demo user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        await SeedCompletedWorkoutAsync(context, demoUser.Id);
        await SeedCurrentWorkoutAsync(context, demoUser.Id);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCompletedWorkoutAsync(HevySyncDbContext context, Guid userId)
    {
        var workoutName = WorkoutName.Create("A2S Hypertrophy - Completed Cycle");
        var exercises = CreateFiveDayWorkoutExercises(Guid.NewGuid(), completed: true);

        var completedWorkout = Workout.Create(workoutName, userId, 5, exercises);
        var workoutSessions = new List<WorkoutSession>();

        for (int week = 1; week <= 21; week++)
        {
            for (int day = 1; day <= 5; day++)
            {
                var dayExercises = completedWorkout.GetExercisesForDay(day);
                var performances = dayExercises.Select(e =>
                {
                    var mockSets = Enumerable.Range(1, e.NumberOfSets)
                        .Select(_ => Set.Create(100m, 8))
                        .ToList();
                    return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
                }).ToList();

                // Create a workout session for this completed day
                var session = WorkoutSession.Create(
                    completedWorkout.Id,
                    week,
                    day,
                    DateTimeOffset.UtcNow.AddDays(-(21 - week) * 7 - (5 - day)), // Past dates
                    performances);
                workoutSessions.Add(session);

                completedWorkout.CompleteDay(performances);
            }

            if (week < 21)
            {
                var weekPerformances = completedWorkout.Exercises.Select(e =>
                {
                    var mockSets = Enumerable.Range(1, e.NumberOfSets)
                        .Select(_ => Set.Create(100m, 8))
                        .ToList();
                    return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
                }).ToList();

                completedWorkout.ApplyProgression(weekPerformances);
            }
        }

        var completedActivity = Activity.Create(userId, completedWorkout.Id, workoutName);
        completedActivity.Complete();

        await context.Workouts.AddAsync(completedWorkout);
        await context.Activities.AddAsync(completedActivity);
        await context.WorkoutSessions.AddRangeAsync(workoutSessions);
    }

    private static async Task SeedCurrentWorkoutAsync(HevySyncDbContext context, Guid userId)
    {
        var workoutName = WorkoutName.Create("A2S Hypertrophy - Current Cycle");
        var workoutId = Guid.NewGuid();
        var exercises = CreateFiveDayWorkoutExercises(workoutId, completed: false);

        var currentWorkout = Workout.Create(workoutName, userId, 5, exercises);
        var workoutSessions = new List<WorkoutSession>();

        for (int week = 1; week < 11; week++)
        {
            for (int day = 1; day <= 5; day++)
            {
                var dayExercises = currentWorkout.GetExercisesForDay(day);
                var performances = dayExercises.Select(e =>
                {
                    var mockSets = Enumerable.Range(1, e.NumberOfSets)
                        .Select(_ => Set.Create(100m, 10))
                        .ToList();
                    return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
                }).ToList();

                // Create a workout session for this completed day
                var session = WorkoutSession.Create(
                    currentWorkout.Id,
                    week,
                    day,
                    DateTimeOffset.UtcNow.AddDays(-(11 - week) * 7 - (5 - day)), // Past dates
                    performances);
                workoutSessions.Add(session);

                currentWorkout.CompleteDay(performances);
            }

            var weekPerformances = currentWorkout.Exercises.Select(e =>
            {
                var mockSets = Enumerable.Range(1, e.NumberOfSets)
                    .Select(_ => Set.Create(100m, 10))
                    .ToList();
                return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
            }).ToList();

            currentWorkout.ApplyProgression(weekPerformances);
        }

        for (int day = 1; day <= 2; day++)
        {
            var dayExercises = currentWorkout.GetExercisesForDay(day);
            var performances = dayExercises.Select(e =>
            {
                var mockSets = Enumerable.Range(1, e.NumberOfSets)
                    .Select(_ => Set.Create(100m, 10))
                    .ToList();
                return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
            }).ToList();

            // Create a workout session for this completed day (current week 11)
            var session = WorkoutSession.Create(
                currentWorkout.Id,
                11,
                day,
                DateTimeOffset.UtcNow.AddDays(-day),
                performances);
            workoutSessions.Add(session);

            currentWorkout.CompleteDay(performances);
        }

        var activeActivity = Activity.Create(userId, currentWorkout.Id, workoutName);

        await context.Workouts.AddAsync(currentWorkout);
        await context.Activities.AddAsync(activeActivity);
        await context.WorkoutSessions.AddRangeAsync(workoutSessions);
    }

    private static async Task SeedWorkoutSessionsForExistingWorkouts(HevySyncDbContext context, Guid userId)
    {
        // Get existing workouts
        var workouts = await context.Workouts
            .Include(w => w.Exercises)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        foreach (var workout in workouts)
        {
            var currentActivity = await context.Activities
                .FirstOrDefaultAsync(a => a.WorkoutId == workout.Id && a.Status != ActivityStatus.Completed);

            if (currentActivity == null)
                continue;

            // Determine how many weeks to generate based on current week
            var currentWeek = workout.Activity.Week;
            var currentDay = workout.Activity.Day;
            var workoutSessions = new List<WorkoutSession>();

            // Generate sessions for completed days (all weeks before current, and days before current in current week)
            for (int week = 1; week <= currentWeek; week++)
            {
                var daysInWeek = week < currentWeek ? workout.Activity.WorkoutsInWeek : currentDay - 1;

                for (int day = 1; day <= daysInWeek; day++)
                {
                    var dayExercises = workout.GetExercisesForDay(day);
                    var performances = dayExercises.Select(e =>
                    {
                        var mockSets = Enumerable.Range(1, e.NumberOfSets)
                            .Select(_ => Set.Create(100m, 10))
                            .ToList();
                        return ExercisePerformance.Create(e.Id, mockSets, PerformanceResult.Success);
                    }).ToList();

                    var daysAgo = (currentWeek - week) * workout.Activity.WorkoutsInWeek + (currentDay - day);
                    var session = WorkoutSession.Create(
                        workout.Id,
                        week,
                        day,
                        DateTimeOffset.UtcNow.AddDays(-daysAgo),
                        performances);
                    workoutSessions.Add(session);
                }
            }

            if (workoutSessions.Any())
            {
                await context.WorkoutSessions.AddRangeAsync(workoutSessions);
            }
        }
    }

    private static async Task SeedWeeklyExercisePlansForExistingWorkouts(HevySyncDbContext context, Guid userId)
    {
        // Get existing workouts
        var workouts = await context.Workouts
            .Where(w => w.UserId == userId)
            .ToListAsync();

        foreach (var workout in workouts)
        {
            var currentActivity = await context.Activities
                .FirstOrDefaultAsync(a => a.WorkoutId == workout.Id && a.Status != ActivityStatus.Completed);

            if (currentActivity == null)
                continue;

            var currentWeek = workout.Activity.Week;

            // Get all exercises for this workout directly from context
            var exercises = await context.Exercises
                .Where(e => e.WorkoutId == workout.Id)
                .ToListAsync();

            // Load progressions for each exercise
            foreach (var exercise in exercises)
            {
                await context.Entry(exercise)
                    .Reference(e => e.Progression)
                    .LoadAsync();
            }

            // Generate planned sets for all exercises in the current week
            var weeklyPlans = new List<WeeklyExercisePlan>();

            foreach (var exercise in exercises)
            {
                // Generate sets based on progression type
                var plannedSets = GeneratePlannedSets(exercise, currentWeek);

                var plan = WeeklyExercisePlan.Create(
                    workout.Id,
                    exercise.Id,
                    currentWeek,
                    plannedSets);

                weeklyPlans.Add(plan);
            }

            if (weeklyPlans.Any())
            {
                await context.WeeklyExercisePlans.AddRangeAsync(weeklyPlans);
            }
        }
    }

    private static List<Set> GeneratePlannedSets(Exercise exercise, int week)
    {
        var sets = new List<Set>();

        switch (exercise.Progression)
        {
            case LinearProgressionStrategy lp:
                // Generate AMRAP sets for linear progression
                var percentage = week == 1 ? 0.85m : 0.85m + ((week - 1) * 0.025m);
                var workingWeight = Math.Round(lp.TrainingMax.Value * percentage / 2.5m) * 2.5m;

                for (int i = 0; i < exercise.NumberOfSets; i++)
                {
                    var reps = i == exercise.NumberOfSets - 1 ? 12 : 5; // Last set is AMRAP, estimate 12 reps
                    sets.Add(Set.Create(workingWeight, reps));
                }
                break;

            case RepsPerSetStrategy rps:
                // Generate sets based on rep range
                for (int i = 0; i < exercise.NumberOfSets; i++)
                {
                    sets.Add(Set.Create(rps.StartingWeight, rps.RepRange.TargetReps));
                }
                break;
        }

        return sets;
    }

    private static List<Exercise> CreateFiveDayWorkoutExercises(Guid workoutId, bool completed)
    {
        var exercises = new List<Exercise>();

        exercises.AddRange(CreateDay1Exercises(workoutId));
        exercises.AddRange(CreateDay2Exercises(workoutId));
        exercises.AddRange(CreateDay3Exercises(workoutId));
        exercises.AddRange(CreateDay4Exercises(workoutId));
        exercises.AddRange(CreateDay5Exercises(workoutId));

        return exercises;
    }

    private static List<Exercise> CreateDay1Exercises(Guid workoutId)
    {
        return new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Squat"),
                "hevy-squat",
                RestTimer.Create(180),
                1, 0, 3, workoutId,
                MuscleGroup.Chest, null,
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(140m),
                    WeightProgression.Create(2.5m),
                    2, true)),
            Exercise.Create(
                ExerciseName.Create("Romanian Deadlift"),
                "hevy-romanian-deadlift",
                RestTimer.Create(120),
                1, 1, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 80m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Leg Press"),
                "hevy-leg-press",
                RestTimer.Create(120),
                1, 2, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 5, 200m,
                    WeightProgression.Create(5m))),
            Exercise.Create(
                ExerciseName.Create("Leg Curl"),
                "hevy-leg-curl",
                RestTimer.Create(90),
                1, 3, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 4, 60m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Calf Raise"),
                "hevy-calf-raise",
                RestTimer.Create(60),
                1, 4, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(12, 15, 20),
                    3, 5, 100m,
                    WeightProgression.Create(5m)))
        };
    }

    private static List<Exercise> CreateDay2Exercises(Guid workoutId)
    {
        return new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Bench Press"),
                "hevy-bench-press",
                RestTimer.Create(180),
                2, 0, 3, workoutId,
                MuscleGroup.Chest, null,
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(100m),
                    WeightProgression.Create(2.5m),
                    2, true)),
            Exercise.Create(
                ExerciseName.Create("Incline Dumbbell Press"),
                "hevy-incline-db-press",
                RestTimer.Create(120),
                2, 1, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 30m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Cable Fly"),
                "hevy-cable-fly",
                RestTimer.Create(90),
                2, 2, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 4, 25m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Tricep Pushdown"),
                "hevy-tricep-pushdown",
                RestTimer.Create(90),
                2, 3, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 4, 35m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Lateral Raise"),
                "hevy-lateral-raise",
                RestTimer.Create(60),
                2, 4, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(12, 15, 20),
                    3, 5, 10m,
                    WeightProgression.Create(1.25m)))
        };
    }

    private static List<Exercise> CreateDay3Exercises(Guid workoutId)
    {
        return new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Deadlift"),
                "hevy-deadlift",
                RestTimer.Create(240),
                3, 0, 3, workoutId,
                MuscleGroup.Chest, null,
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(160m),
                    WeightProgression.Create(2.5m),
                    2, true)),
            Exercise.Create(
                ExerciseName.Create("Front Squat"),
                "hevy-front-squat",
                RestTimer.Create(120),
                3, 1, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(6, 8, 10),
                    3, 5, 80m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Bulgarian Split Squat"),
                "hevy-bulgarian-split-squat",
                RestTimer.Create(90),
                3, 2, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 4, 20m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Leg Extension"),
                "hevy-leg-extension",
                RestTimer.Create(90),
                3, 3, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 4, 70m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Ab Wheel"),
                "hevy-ab-wheel",
                RestTimer.Create(90),
                3, 4, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 0m,
                    WeightProgression.Create(1m)))
        };
    }

    private static List<Exercise> CreateDay4Exercises(Guid workoutId)
    {
        return new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Overhead Press"),
                "hevy-ohp",
                RestTimer.Create(180),
                4, 0, 3, workoutId,
                MuscleGroup.Chest, null,
                LinearProgressionStrategy.Create(
                    Guid.Empty,
                    TrainingMax.Create(60m),
                    WeightProgression.Create(1.25m),
                    2, true)),
            Exercise.Create(
                ExerciseName.Create("Pull-ups"),
                "hevy-pullups",
                RestTimer.Create(120),
                4, 1, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(6, 8, 10),
                    3, 5, 0m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Barbell Row"),
                "hevy-barbell-row",
                RestTimer.Create(120),
                4, 2, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 80m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Face Pull"),
                "hevy-face-pull",
                RestTimer.Create(90),
                4, 3, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(12, 15, 20),
                    3, 4, 30m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Bicep Curl"),
                "hevy-bicep-curl",
                RestTimer.Create(90),
                4, 4, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 4, 15m,
                    WeightProgression.Create(1.25m)))
        };
    }

    private static List<Exercise> CreateDay5Exercises(Guid workoutId)
    {
        return new List<Exercise>
        {
            Exercise.Create(
                ExerciseName.Create("Pause Squat"),
                "hevy-pause-squat",
                RestTimer.Create(150),
                5, 0, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(5, 6, 8),
                    3, 5, 100m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Dumbbell Bench Press"),
                "hevy-db-bench",
                RestTimer.Create(120),
                5, 1, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 35m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Lat Pulldown"),
                "hevy-lat-pulldown",
                RestTimer.Create(90),
                5, 2, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(10, 12, 15),
                    3, 5, 70m,
                    WeightProgression.Create(2.5m))),
            Exercise.Create(
                ExerciseName.Create("Dumbbell Shoulder Press"),
                "hevy-db-shoulder-press",
                RestTimer.Create(90),
                5, 3, 4, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(8, 10, 12),
                    3, 5, 25m,
                    WeightProgression.Create(1.25m))),
            Exercise.Create(
                ExerciseName.Create("Plank"),
                "hevy-plank",
                RestTimer.Create(60),
                5, 4, 3, workoutId,
                MuscleGroup.Chest, null,
                RepsPerSetStrategy.Create(
                    Guid.Empty,
                    RepRange.Create(30, 45, 60),
                    3, 4, 0m,
                    WeightProgression.Create(1m)))
        };
    }
}

