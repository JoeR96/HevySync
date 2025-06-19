using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

internal static class RepsPerSetExerciseDetailsRequestBogusGenerator
{
    public static Faker<RepsPerSetExerciseDetailsRequest> GenerateRepsPerSetExerciseDetailsRequest()
    {
        return new Faker<RepsPerSetExerciseDetailsRequest>()
            .CustomInstantiator(f => new RepsPerSetExerciseDetailsRequest(
                f.Random.Number(1, 5),
                f.Random.Number(6, 10),
                f.Random.Number(11, 15),
                f.Random.Number(2, 6),
                f.Random.Number(6, 10),
                ExerciseProgram.Average2SavageRepsPerSet
            ));
    }
}