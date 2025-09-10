using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.IntegrationTests.Bogus;
using HevySync.IntegrationTests.Extensions;
using HevySync.IntegrationTests.Fixtures;
using HevySync.Models;
using Shouldly;

namespace HevySync.IntegrationTests.Tests.Average2SavageWorkoutTests;

[Collection("Workout Integration Tests")]
public class WorkoutFlowTests(WebHostFixture webHostFixture) : IClassFixture<WebHostFixture>
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();
    private WorkoutDto? _createdWorkout;
    private CreateWorkoutRequest? _validWorkoutRequest;
    private WeeklyWorkoutPlanDto? _weekOneWorkout;

    [Fact]
    public async Task CreateWorkout_ShouldSucceed()
    {
        var createdWorkout = await EnsureWorkoutCreated();

        createdWorkout.ShouldNotBeNull();
        createdWorkout.Name.ShouldBe(GetValidWorkoutRequest().WorkoutName);
    }

    [Fact]
    public async Task GenerateWeekOne_ShouldCreateSessionExercises()
    {
        var weekOne = await EnsureWeekOneGenerated();

        weekOne.ShouldNotBeNull();
        weekOne.WorkoutId.ShouldBe(_createdWorkout!.Id);
        weekOne.Week.ShouldBe(1);
    }

    [Fact]
    public async Task GetWeekOne_ShouldReturnGeneratedData()
    {
        await EnsureWeekOneGenerated();
        var getWeekOneEndpoint = Average2SavageEndpoint.WorkoutGetCurrentWeek.GetFullRoutePath();

        var retrievedWeekOne = await _client.GetAsync<WeeklyWorkoutPlanDto>(
            $"{getWeekOneEndpoint}?workoutId={_createdWorkout!.Id}");

        retrievedWeekOne.ShouldNotBeNull();
        retrievedWeekOne.WorkoutId.ShouldBe(_weekOneWorkout!.WorkoutId);
        retrievedWeekOne.Week.ShouldBe(_weekOneWorkout.Week);
    }

    private async Task<WorkoutDto> EnsureWorkoutCreated()
    {
        if (_createdWorkout == null)
        {
            var request = GetValidWorkoutRequest();
            var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();
            _createdWorkout = await _client.PostAsync<WorkoutDto>(endpoint, request);
        }

        return _createdWorkout;
    }

    private async Task<WeeklyWorkoutPlanDto> EnsureWeekOneGenerated()
    {
        if (_weekOneWorkout == null)
        {
            await EnsureWorkoutCreated();
            var createWeekOneEndpoint = Average2SavageEndpoint.WorkoutCreateWeekOne.GetFullRoutePath();
            var request = new SyncHevyWorkoutsRequest { WorkoutId = _createdWorkout!.Id };
            _weekOneWorkout = await _client.PostAsync<WeeklyWorkoutPlanDto>(createWeekOneEndpoint, request);
        }

        return _weekOneWorkout;
    }

    private CreateWorkoutRequest GetValidWorkoutRequest()
    {
        if (_validWorkoutRequest == null)
        {
            var exerciseRequestsFaker = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
                RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
                LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
                5,
                5
            );

            _validWorkoutRequest = new CreateWorkoutRequest
            {
                WorkoutName = "Flow Test Workout",
                Exercises = exerciseRequestsFaker,
                WorkoutDaysInWeek = 5
            };
        }

        return _validWorkoutRequest;
    }
}