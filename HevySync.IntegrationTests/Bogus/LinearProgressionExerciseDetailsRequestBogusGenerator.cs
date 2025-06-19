using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

internal static class LinearProgressionExerciseDetailsRequestBogusGenerator
{
    public static Faker<LinearProgressionExerciseDetailsRequest> GenerateLinearProgressionExerciseDetailsRequest()
    {
        return new Faker<LinearProgressionExerciseDetailsRequest>()
            .CustomInstantiator(f => new LinearProgressionExerciseDetailsRequest(
                100.00m,
                Math.Round(f.Random.Decimal(1.0m, 10.0m), 2),
                f.Random.Number(3, 7),
                ExerciseProgram.Average2SavageHypertrophy,
                f.PickRandom<BodyCategory>(),
                f.PickRandom<EquipmentType>()
            ));
    }
}