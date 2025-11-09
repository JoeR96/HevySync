using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.IntegrationTests.Bogus;

internal static class RepsPerSetExerciseDetailsRequestBogusGenerator
{
    public static Faker<RepsPerSetExerciseDetailsRequest> GenerateRepsPerSetExerciseDetailsRequest()
    {
        return new Faker<RepsPerSetExerciseDetailsRequest>()
            .CustomInstantiator(f => new RepsPerSetExerciseDetailsRequest(
                f.Random.Number(1, 5),          // MinimumReps
                f.Random.Number(6, 10),         // TargetReps
                f.Random.Number(11, 15),        // MaximumTargetReps
                f.Random.Number(2, 6),          // NumberOfSets
                f.Random.Number(6, 10),         // TotalNumberOfSets
                f.Random.Decimal(1, 20),        // StartingWeight
                f.Random.Decimal(2.5m, 5m),     // WeightProgression
                ExerciseProgram.Average2SavageRepsPerSet
            ));
    }
}