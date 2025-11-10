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
public class WorkoutWeekSessionsTests(WebHostFixture webHostFixture)
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();

    [Fact]
    public async Task GetWorkoutWeekSessions_WhenNoSessionsExist_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var workout = await CreateWorkout();
        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetWorkoutWeekSessions_AfterGeneratingWeekOne_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);
        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldBeEmpty(); // Sessions are only created after completing a day
    }

    [Fact]
    public async Task GetWorkoutWeekSessions_AfterCompletingOneDay_ShouldReturnSessionForWeekOne()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);
        await CompleteDay(workout);
        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldNotBeEmpty();
        sessions.ShouldContainKey(1); // Week 1
        sessions[1].ShouldNotBeEmpty();
        sessions[1].Count.ShouldBe(1);

        var session = sessions[1][0];
        session.WorkoutId.ShouldBe(workout.Id);
        session.Week.ShouldBe(1);
        session.Day.ShouldBe(1);
        session.ExercisePerformances.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetWorkoutWeekSessions_AfterCompletingMultipleDays_ShouldReturnAllSessionsGroupedByWeek()
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
        sessions.ShouldNotBeEmpty();
        sessions.ShouldContainKey(1); // Week 1
        sessions[1].Count.ShouldBe(3);

        // Verify sessions are ordered by day
        sessions[1][0].Day.ShouldBe(1);
        sessions[1][1].Day.ShouldBe(2);
        sessions[1][2].Day.ShouldBe(3);

        // Verify all sessions have exercise performances
        foreach (var session in sessions[1])
        {
            session.WorkoutId.ShouldBe(workout.Id);
            session.Week.ShouldBe(1);
            session.ExercisePerformances.ShouldNotBeEmpty();
        }
    }

    [Fact]
    public async Task GetWorkoutWeekSessions_SessionsContainExerciseDetails_ShouldIncludeNameAndSets()
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
        var exercisePerformances = session.ExercisePerformances;

        exercisePerformances.ShouldNotBeEmpty();

        foreach (var performance in exercisePerformances)
        {
            performance.ExerciseName.ShouldNotBeNullOrWhiteSpace();
            performance.ExerciseTemplateId.ShouldNotBeNull(); // Can be empty string but not null
            performance.PerformanceResult.ShouldNotBeNullOrWhiteSpace();
            performance.CompletedSets.ShouldNotBeEmpty();

            foreach (var set in performance.CompletedSets)
            {
                set.WeightKg.ShouldBeGreaterThanOrEqualTo(0);
                set.Reps.ShouldBeGreaterThan(0);
            }
        }
    }

    [Fact]
    public async Task GetWorkoutWeekSessions_AfterCompletingEntireWeek_ShouldHaveAllDays()
    {
        // Arrange
        var workout = await CreateWorkout();
        await GenerateWeekOne(workout.Id);

        // Complete all 5 days (based on the CreateWorkout method)
        for (int i = 0; i < workout.WorkoutActivity.WorkoutsInWeek; i++)
        {
            await CompleteDay(workout);
        }

        var endpoint = $"average2savage/workout/{workout.Id}/week-sessions";

        // Act
        var sessions = await _client.GetAsync<Dictionary<int, List<WorkoutSessionDto>>>(endpoint);

        // Assert
        sessions.ShouldNotBeNull();
        sessions.ShouldContainKey(1);
        sessions[1].Count.ShouldBe(workout.WorkoutActivity.WorkoutsInWeek);

        // Verify each day is present
        for (int day = 1; day <= workout.WorkoutActivity.WorkoutsInWeek; day++)
        {
            sessions[1].ShouldContain(s => s.Day == day);
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
            WorkoutName = $"Week Sessions Test Workout {Guid.NewGuid()}",
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
