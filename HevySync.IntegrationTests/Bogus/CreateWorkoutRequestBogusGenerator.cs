using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

public static class CreateWorkoutRequestBogusGenerator
{
    public static Faker<CreateWorkoutRequest> GenerateCreateWorkoutRequest(
        Faker<ExerciseRequest> exerciseGenerator)
    {
        return new Faker<CreateWorkoutRequest>()
            .RuleFor(w => w.WorkoutName, f => f.Company.CatchPhrase())
            .RuleFor(w => w.Exercises,
                f => exerciseGenerator.Generate(f.Random.Number(3, 10)));
    }
}