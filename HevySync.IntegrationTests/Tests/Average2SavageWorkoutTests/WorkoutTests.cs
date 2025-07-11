using HevySync.Endpoints.Average2Savage;
using HevySync.Endpoints.Average2Savage.Enums;
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
            Exercises = exerciseRequestsFaker,
            WorkoutDaysInWeek = 5
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var response = await _client.PostAsync<WorkoutDto>(
            endpoint,
            workoutRequest);

        response!.Exercises.Count.ShouldBe(workoutRequest.Exercises.Count);
        response!.Name.ShouldBe(workoutRequest.WorkoutName);

        var exerciseTasks = workoutRequest.Exercises.Zip(response.Exercises, (request, response) => (request, response))
            .Select(async exercisePair =>
            {
                var (requestExercise, responseExercise) = exercisePair;

                responseExercise.ExerciseName.ShouldBe(requestExercise.ExerciseName);
                responseExercise.Day.ShouldBe(requestExercise.Day);
                responseExercise.RestTimer.ShouldBe(requestExercise.RestTimer);

                switch (responseExercise.ExerciseDetail)
                {
                    case LinearProgressionDto linearResponse:
                        var linearRequest =
                            (LinearProgressionExerciseDetailsRequest)requestExercise.ExerciseDetailsRequest;
                        linearResponse.WeightProgression.ShouldBe(linearRequest.WeightProgression);
                        linearResponse.AttemptsBeforeDeload.ShouldBe(linearRequest.AttemptsBeforeDeload);
                        break;

                    case RepsPerSetDto repsResponse:
                        var repsRequest = (RepsPerSetExerciseDetailsRequest)requestExercise.ExerciseDetailsRequest;
                        repsResponse.MinimumReps.ShouldBe(repsRequest.MinimumReps);
                        repsResponse.TargetReps.ShouldBe(repsRequest.TargetReps);
                        repsResponse.MaximumTargetReps.ShouldBe(repsRequest.MaximumTargetReps);
                        repsResponse.StartingSetCount.ShouldBe(repsRequest.NumberOfSets);
                        repsResponse.TargetSetCount.ShouldBe(repsRequest.TotalNumberOfSets);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unexpected exercise detail type in response: {responseExercise.ExerciseDetail?.GetType().Name}");
                }
            });

        await Task.WhenAll(exerciseTasks);
    }

    [Fact]
    public async Task CreateWorkoutWithInvalidPropertiesPresentsValidationFailures()
    {
        var exerciseRequestsFaker = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            5,
            5
        );

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "",
            Exercises = exerciseRequestsFaker
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse!.Errors.Count.ShouldBe(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "WorkoutName" &&
            e.Message == "Workout name cannot be empty.");
    }

    [Fact]
    public async Task CreateWorkout_WithEmptyExercises_ShouldReturnValidationErrorAsync()
    {
        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "Morning Workout",
            Exercises = new List<ExerciseRequest>()
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse.Errors.Count.ShouldBe(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "Exercises" &&
            e.Message == "A workout must contain at least one exercise.");
    }

    [Fact]
    public async Task CreateWorkout_WithEmptyExerciseName_ShouldReturnValidationErrorAsync()
    {
        var exerciseRequests = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            5,
            1);

        exerciseRequests[0].ExerciseName = "";

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "Morning Workout",
            Exercises = exerciseRequests
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse.Errors.Count.ShouldBe(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "Exercises[0].ExerciseName" &&
            e.Message == "Exercise name cannot be empty.");
    }

    [Fact]
    public async Task CreateWorkout_WithInvalidRepsPerSetExerciseDetails_ShouldReturnValidationErrorAsync()
    {
        var repsPerSetRequest = RepsPerSetExerciseDetailsRequestBogusGenerator
            .GenerateRepsPerSetExerciseDetailsRequest()
            .Generate();

        repsPerSetRequest = repsPerSetRequest with { MinimumReps = 0 };

        var exerciseRequests = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            new Faker<RepsPerSetExerciseDetailsRequest>().CustomInstantiator(_ => repsPerSetRequest),
            LinearProgressionExerciseDetailsRequestBogusGenerator.GenerateLinearProgressionExerciseDetailsRequest(),
            3,
            1);

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "Evening Workout",
            Exercises = exerciseRequests
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse.Errors.Count.ShouldBeGreaterThanOrEqualTo(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property.Contains("MinimumReps") &&
            e.Message == "Minimum reps must be at least 1.");
    }

    [Fact]
    public async Task CreateWorkout_WithInvalidLinearProgressionDetails_ShouldReturnValidationErrorAsync()
    {
        var linearProgressionRequest = LinearProgressionExerciseDetailsRequestBogusGenerator
            .GenerateLinearProgressionExerciseDetailsRequest()
            .Generate();

        linearProgressionRequest = linearProgressionRequest with { WeightProgression = -1.5m };

        var exerciseRequests = ExerciseRequestBogusGenerator.GenerateExerciseRequests(
            RepsPerSetExerciseDetailsRequestBogusGenerator.GenerateRepsPerSetExerciseDetailsRequest(),
            new Faker<LinearProgressionExerciseDetailsRequest>().CustomInstantiator(_ => linearProgressionRequest),
            3,
            1);

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "Strength Workout",
            Exercises = exerciseRequests
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse.Errors.Count.ShouldBeGreaterThanOrEqualTo(1);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property.Contains("WeightProgression") &&
            e.Message == "Weight progression must be greater than 0.");
    }

    [Fact]
    public async Task CreateWorkout_WithMultipleInvalidProperties_ShouldReturnValidationErrorsAsync()
    {
        var exerciseRequests = new List<ExerciseRequest>
        {
            new()
            {
                ExerciseName = "",
                Day = 0,
                Order = 1,
                ExerciseDetailsRequest = new LinearProgressionExerciseDetailsRequest(
                    25m,
                    -5m,
                    3,
                    ExerciseProgram.Average2SavageHypertrophy,
                    BodyCategory.Chest,
                    EquipmentType.Barbell)
            }
        };

        var workoutRequest = new CreateWorkoutRequest
        {
            WorkoutName = "",
            Exercises = exerciseRequests
        };

        var endpoint = Average2SavageEndpoint.Workout.GetFullRoutePath();

        var validationErrorResponse = await _client.PostAndAssertValidationErrorAsync(
            endpoint,
            workoutRequest);

        validationErrorResponse.Errors.Count.ShouldBeGreaterThanOrEqualTo(3);
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "WorkoutName" &&
            e.Message == "Workout name cannot be empty.");
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "Exercises[0].ExerciseName" &&
            e.Message == "Exercise name cannot be empty.");
        validationErrorResponse.Errors.ShouldContain(e =>
            e.Property == "Exercises[0].ExerciseDetailsRequest.WeightProgression" &&
            e.Message == "Weight progression must be greater than 0.");
    }
}