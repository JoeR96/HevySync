using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

internal static class ExerciseRequestBogusGenerator
{
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
                RestTimer = new Faker().Random.Number(60, 120),
                ExerciseTemplateId = "05293BCA",
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