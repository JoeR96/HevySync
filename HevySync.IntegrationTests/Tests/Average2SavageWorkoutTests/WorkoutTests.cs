using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.IntegrationTests.Bogus;
using HevySync.IntegrationTests.Extensions;
using Shouldly;

namespace HevySync.IntegrationTests.Tests.Average2SavageWorkoutTests;

public class WorkoutTests(
    WebHostFixture webHostFixture)
    : IClassFixture<WebHostFixture>
{
    private readonly HttpClient _client = webHostFixture.GetHttpClient();

    [Fact]
    public async Task CreateWorkout()
    {
        var exerciseRequestsFaker = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            5,
            5
        );

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "Test Workout",
            Exercises = exerciseRequestsFaker
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var response = await _client.PostAsync<WorkoutDto>(
            endpoint,
            workoutRequest);

        response!.Exercises.Count.ShouldBe(workoutRequest.Exercises.Count);
        response!.Name.ShouldBe(workoutRequest.WorkoutName);
        for (var i = 0; i < workoutRequest.Exercises.Count; i++)
        {
            var requestExercise = workoutRequest.Exercises[i];
            var responseExercise = response.Exercises[i];

            responseExercise.ExerciseName.ShouldBe(requestExercise.ExerciseName);
            responseExercise.Day.ShouldBe(requestExercise.Day);

            switch (responseExercise.ExerciseDetail)
            {
                case LinearProgressionDto linearResponse:
                    var linearRequest = (LinearProgressionExerciseDetailsRequest)requestExercise.ExerciseDetailsRequest;
                    linearResponse.WeightProgression.ShouldBe(linearRequest.WeightProgression);
                    linearResponse.AttemptsBeforeDeload.ShouldBe(linearRequest.AttemptsBeforeDeload);
                    break;

                case RepsPerSetDto repsResponse:
                    var repsRequest = (RepsPerSetExerciseDetailsRequest)requestExercise.ExerciseDetailsRequest;
                    repsResponse.MinimumReps.ShouldBe(repsRequest.MinimumReps);
                    repsResponse.TargetReps.ShouldBe(repsRequest.TargetReps);
                    repsResponse.MaximumTargetReps.ShouldBe(repsRequest.MaximumTargetReps);
                    repsResponse.NumberOfSets.ShouldBe(repsRequest.NumberOfSets);
                    repsResponse.TotalNumberOfSets.ShouldBe(repsRequest.TotalNumberOfSets);
                    break;

                default:
                    throw new InvalidOperationException("Unexpected exercise detail type in response.");
            }
        }
    }
}