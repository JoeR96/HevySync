using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.IntegrationTests.Bogus;
using HevySync.Models;

namespace HevySync.IntegrationTests.Fixtures;

[CollectionDefinition("Workout Flow")]
public class WorkoutFlowCollection : ICollectionFixture<WorkoutFlowFixture>
{
}

public class WorkoutFlowFixture : IDisposable
{
    public WorkoutFlowFixture(WorkoutDto createdWorkout, WeeklyWorkoutPlanDto weekOneWorkout)
    {
        CreatedWorkout = createdWorkout;
        WeekOneWorkout = weekOneWorkout;
        var webHostFixture = new WebHostFixture();
        Client = webHostFixture.GetHttpClient();
        ValidWorkoutRequest = CreateValidWorkoutRequest();
    }

    private HttpClient Client { get; }
    private CreateWorkoutRequest ValidWorkoutRequest { get; }
    private WorkoutDto CreatedWorkout { get; set; }
    private WeeklyWorkoutPlanDto WeekOneWorkout { get; set; }

    public void Dispose()
    {
        Client?.Dispose();
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
            WorkoutName = "Flow Test Workout",
            Exercises = exerciseRequestsFaker,
            WorkoutDaysInWeek = 5
        };
    }
}