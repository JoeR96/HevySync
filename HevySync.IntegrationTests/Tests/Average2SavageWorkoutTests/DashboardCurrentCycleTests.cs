using HevySync.Application.DTOs;
using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.IntegrationTests.Bogus;
using HevySync.IntegrationTests.Extensions;
using HevySync.IntegrationTests.Fixtures;
using HevySync.Models;
using Shouldly;

namespace HevySync.IntegrationTests.Tests.Average2SavageWorkoutTests;

[Collection("Integration Tests")]
public class DashboardCurrentCycleTests(WebHostFixture webHostFixture)
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();

    [Fact]
    public async Task DashboardCurrentCycle_WhenNoWorkoutSessionsExist_ShouldReturnEmptyResult()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);
        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldBeEmpty();
    }

    [Fact]
    public async Task DashboardCurrentCycle_AfterCompletingCurrentWeekDays_ShouldReturnCurrentWeekSessions()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        // Complete 2 days in week 1
        await CompleteDay(workout);
        await CompleteDay(workout);

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldNotBeEmpty();

        // Current week should be week 1
        sessions.ShouldContainKey(1);
        sessions[1].Count.ShouldBe(2);

        // Verify the sessions contain exercise data for dashboard display
        foreach (var session in sessions[1])
        {
            session.Week.ShouldBe(1);
            session.ExercisePerformances.ShouldNotBeEmpty();

            foreach (var performance in session.ExercisePerformances)
            {
                // Dashboard needs exercise name
                performance.ExerciseName.ShouldNotBeNullOrWhiteSpace();

                // Dashboard needs exercise template ID
                performance.ExerciseTemplateId.ShouldNotBeNull();

                // Dashboard needs sets information
                performance.CompletedSets.ShouldNotBeEmpty();

                // Each set should have weight and reps
                foreach (var set in performance.CompletedSets)
                {
                    set.WeightKg.ShouldBeGreaterThanOrEqualTo(0);
                    set.Reps.ShouldBeGreaterThan(0);
                }
            }
        }
    }

    [Fact]
    public async Task DashboardCurrentCycle_ShouldShowAllExercisesPerformedInCurrentWeek()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        // Complete 3 days
        await CompleteDay(workout);
        await CompleteDay(workout);
        await CompleteDay(workout);

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldContainKey(1);

        var week1Sessions = sessions[1];
        week1Sessions.Count.ShouldBe(3);

        // Collect all unique exercises performed in the current week
        var allExercisesInWeek = week1Sessions
            .SelectMany(s => s.ExercisePerformances)
            .Select(ep => ep.ExerciseId)
            .Distinct()
            .ToList();

        allExercisesInWeek.ShouldNotBeEmpty();

        // Verify each exercise has complete data
        var allPerformances = week1Sessions
            .SelectMany(s => s.ExercisePerformances)
            .ToList();

        foreach (var performance in allPerformances)
        {
            // Essential data for dashboard widget
            performance.ExerciseId.ShouldNotBe(Guid.Empty);
            performance.ExerciseName.ShouldNotBeNullOrWhiteSpace();
            performance.ExerciseTemplateId.ShouldNotBeNull(); // Can be empty string but not null
            performance.PerformanceResult.ShouldNotBeNullOrWhiteSpace();
            performance.CompletedSets.ShouldNotBeEmpty();
        }
    }

    [Fact]
    public async Task DashboardCurrentCycle_VerifySessionsAreOrderedByDay()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        // Complete 4 days
        for (int i = 0; i < 4; i++)
        {
            await CompleteDay(workout);
        }

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldContainKey(1);

        var week1Sessions = sessions[1];
        week1Sessions.Count.ShouldBe(4);

        // Verify sessions are ordered by day (important for dashboard display)
        for (int i = 0; i < week1Sessions.Count; i++)
        {
            week1Sessions[i].Day.ShouldBe(i + 1);
        }
    }

    [Fact]
    public async Task DashboardCurrentCycle_VerifyCompletedAtTimestamp()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        var beforeCompletion = DateTimeOffset.UtcNow;
        await CompleteDay(workout);
        var afterCompletion = DateTimeOffset.UtcNow;

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldContainKey(1);

        var session = sessions[1][0];

        // Verify CompletedAt is set and within reasonable bounds
        session.CompletedAt.ShouldBeGreaterThanOrEqualTo(beforeCompletion);
        session.CompletedAt.ShouldBeLessThanOrEqualTo(afterCompletion);
    }

    [Fact]
    public async Task DashboardCurrentCycle_MultipleWeeks_ShouldOnlyReturnRequestedWeek()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        // Complete all days of week 1
        for (int i = 0; i < workout.WorkoutActivity.WorkoutsInWeek; i++)
        {
            await CompleteDay(workout);
        }

        // Generate and complete some days in week 2
        await GenerateNextWeek(workout.Id);
        await CompleteDay(workout);
        await CompleteDay(workout);

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();

        // Should have both weeks
        sessions.ShouldContainKey(1);
        sessions.ShouldContainKey(2);

        // Week 1 should have all days
        sessions[1].Count.ShouldBe(workout.WorkoutActivity.WorkoutsInWeek);

        // Week 2 should have 2 days
        sessions[2].Count.ShouldBe(2);

        // Each week's sessions should only contain their respective week number
        sessions[1].ShouldAllBe(s => s.Week == 1);
        sessions[2].ShouldAllBe(s => s.Week == 2);
    }

    [Fact]
    public async Task DashboardCurrentCycle_VerifyExerciseDataMatchesWorkoutDefinition()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);
        await CompleteDay(workout);

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        var session = sessions[1][0];
        var performedExerciseIds = session.ExercisePerformances.Select(ep => ep.ExerciseId).ToList();

        // Get expected exercises for day 1
        var expectedExerciseIds = workout.Exercises
            .Where(e => e.Day == 1)
            .Select(e => e.Id)
            .ToList();

        // Verify all expected exercises are in the session
        foreach (var expectedId in expectedExerciseIds)
        {
            performedExerciseIds.ShouldContain(expectedId);
        }

        // Verify exercise names match
        foreach (var performance in session.ExercisePerformances)
        {
            var expectedExercise = workout.Exercises.First(e => e.Id == performance.ExerciseId);
            performance.ExerciseName.ShouldBe(expectedExercise.ExerciseName);
        }
    }

    private async Task<Models.WorkoutDto> CreateWorkout()
    {
        var exerciseRequestsFaker = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            5,
            5
        );

        var request = new CreateWorkoutRequest
        {
            WorkoutName = $"Dashboard Cycle Test Workout {Guid.NewGuid()}",
            Exercises = exerciseRequestsFaker,
            WorkoutDaysInWeek = 5
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();
        return await _client.PostAsync<Models.WorkoutDto>(endpoint, request);
    }

    private async Task<WeeklyWorkoutPlanDto> GenerateWeekOne(Guid workoutId)
    {
        var endpoint = Average2SavageEndpoint.WorkoutCreateWeekOne.GetFullRoutePath();
        var request = new SyncHevyWorkoutsRequest { WorkoutId = workoutId };
        return await _client.PostAsync<WeeklyWorkoutPlanDto>(endpoint, request);
    }

    private async Task<WeeklyWorkoutPlanDto> GenerateNextWeek(Guid workoutId)
    {
        var endpoint = Average2SavageEndpoint.WorkoutGenerateNextWeek.GetFullRoutePath();

        // For generating next week, we need week performances with dummy data
        var request = new GenerateNextWeekRequest
        {
            WorkoutId = workoutId,
            WeekPerformances = new List<ExercisePerformanceRequest>
            {
                new ExercisePerformanceRequest
                {
                    ExerciseId = Guid.NewGuid(),
                    PerformanceResult = "Success",
                    CompletedSets = new List<CompletedSetRequest>
                    {
                        new CompletedSetRequest { WeightKg = 100, Reps = 10 }
                    }
                }
            }
        };

        return await _client.PostAsync<WeeklyWorkoutPlanDto>(endpoint, request);
    }

    private async Task CompleteDay(Models.WorkoutDto workout)
    {
        var endpoint = Average2SavageEndpoint.WorkoutCompleteDay.GetFullRoutePath();

        // Get exercises for the current day
        var dayExercises = workout.Exercises.Where(e => e.Day == workout.WorkoutActivity.Day).ToList();

        var exercisePerformances = dayExercises.Select(e => new ExercisePerformanceRequest
        {
            ExerciseId = e.Id,
            PerformanceResult = "Success",
            CompletedSets = Enumerable.Range(1, e.NumberOfSets)
                .Select(_ => new CompletedSetRequest
                {
                    WeightKg = 100,
                    Reps = 10
                }).ToList()
        }).ToList();

        var request = new CompleteWorkoutDayRequest
        {
            WorkoutId = workout.Id,
            ExercisePerformances = exercisePerformances
        };

        var result = await _client.PostAsync<Endpoints.Average2Savage.Responses.CompleteWorkoutDayResponse>(endpoint, request);

        // Update the workout object to reflect the new state
        workout.WorkoutActivity.Week = result.NewWeek;
        workout.WorkoutActivity.Day = result.NewDay;
    }
}
