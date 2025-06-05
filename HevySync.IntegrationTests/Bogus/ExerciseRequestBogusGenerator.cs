using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

internal static class ExerciseRequestBogusGenerator
{
    public static Faker<ExerciseRequest> GenerateExerciseRequest(
        Faker<RepsPerSetExerciseDetailsRequest> repsGenerator,
        Faker<LinearProgressionExerciseDetailsRequest> linearProgressionGenerator)
    {
        return new Faker<ExerciseRequest>()
            .RuleFor(e => e.ExerciseName, f => f.Lorem.Word())
            .RuleFor(e => e.Day, f => f.Random.Number(1, 7))
            .RuleFor(e => e.Order, f => f.Random.Number(1, 10))
            .RuleFor(e => e.ExerciseDetailsRequest, f =>
                f.Random.Bool()
                    ? repsGenerator.Generate()
                    : linearProgressionGenerator.Generate()
            );
    }

    public static List<ExerciseRequest> GenerateExerciseRequests(
        Faker<RepsPerSetExerciseDetailsRequest> repsGenerator,
        Faker<LinearProgressionExerciseDetailsRequest> linearProgressionGenerator,
        int numberOfExercisesPerDay,
        int numberOfDays)
    {
        var exercises = new List<ExerciseRequest>();

        for (var day = 1; day <= numberOfDays; day++)
        for (var order = 1; order <= numberOfExercisesPerDay; order++)
        {
            var exercise = new ExerciseRequest
            {
                ExerciseName = new Faker().Lorem.Word(),
                Day = day,
                Order = order,
                ExerciseDetailsRequest = new Faker().Random.Bool()
                    ? repsGenerator.Generate()
                    : linearProgressionGenerator.Generate()
            };

            exercises.Add(exercise);
        }

        return exercises;
    }
}