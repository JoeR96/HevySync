using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.IntegrationTests.Bogus;
using HevySync.IntegrationTests.Extensions;
using HevySync.IntegrationTests.Fixtures;
using HevySync.Models;
using HevySync.Models.Exercises;
using Shouldly;

namespace HevySync.IntegrationTests.Tests.Average2SavageWorkoutTests;

[Collection("Integration Tests")]
public class WorkoutCreationTests(WebHostFixture webHostFixture)
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();

    [Fact]
    public async Task CreateWorkout_WithValidData_ShouldReturnWorkoutDto()
    {
        var request = CreateValidWorkoutRequest();
        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var response = await _client.PostAsync<WorkoutDto>(endpoint, request);

        response.ShouldNotBeNull();
        AssertWorkoutResponseMatchesRequest(response, request);
    }

    private CreateWorkoutRequest CreateValidWorkoutRequest()
    {
        var exerciseRequestsFaker = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            5,
            5
        );

        return new CreateWorkoutRequest
        {
            WorkoutName = "Test Workout",
            Exercises = exerciseRequestsFaker,
            WorkoutDaysInWeek = 5
        };
    }

    private void AssertWorkoutResponseMatchesRequest(WorkoutDto response, CreateWorkoutRequest request)
    {
        response.Exercises.Count.ShouldBe(request.Exercises.Count);
        response.Name.ShouldBe(request.WorkoutName);

        foreach (var (requestExercise, responseExercise) in request.Exercises.Zip(response.Exercises))
            AssertExerciseResponseMatchesRequest(responseExercise, requestExercise);
    }

    private void AssertExerciseResponseMatchesRequest(ExerciseDto response, ExerciseRequest request)
    {
        response.ExerciseName.ShouldBe(request.ExerciseName);
        response.Day.ShouldBe(request.Day);
        response.RestTimer.ShouldBe(request.RestTimer);
        response.Order.ShouldBe(request.Order);

        switch (response.ExerciseDetail)
        {
            case LinearProgressionDto linearResponse:
                var linearRequest = (LinearProgressionExerciseDetailsRequest)request.ExerciseDetailsRequest;
                linearResponse.WeightProgression.ShouldBe(linearRequest.WeightProgression);
                linearResponse.AttemptsBeforeDeload.ShouldBe(linearRequest.AttemptsBeforeDeload);
                linearResponse.TrainingMax.ShouldBe(linearRequest.TrainingMax);
                break;

            case RepsPerSetDto repsResponse:
                var repsRequest = (RepsPerSetExerciseDetailsRequest)request.ExerciseDetailsRequest;
                repsResponse.MinimumReps.ShouldBe(repsRequest.MinimumReps);
                repsResponse.TargetReps.ShouldBe(repsRequest.TargetReps);
                repsResponse.MaximumTargetReps.ShouldBe(repsRequest.MaximumTargetReps);
                repsResponse.StartingSetCount.ShouldBe(repsRequest.NumberOfSets);
                repsResponse.TargetSetCount.ShouldBe(repsRequest.TotalNumberOfSets);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unexpected exercise detail type: {response.ExerciseDetail?.GetType().Name}");
        }
    }
}