using System.Text.Json;
using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;

namespace HevySync.Endpoints.Average2Savage.Converters;

public class ExerciseDetailsRequestConverter : JsonConverter<ExerciseDetailsRequest>
{
    private const string ProgramDiscriminator = "Program";

    public override ExerciseDetailsRequest Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty(ProgramDiscriminator, out var programElement))
            throw new JsonException(
                $"Missing '{ProgramDiscriminator}' field in JSON payload for polymorphic deserialization.");

        var program = programElement.GetString();
        if (string.IsNullOrEmpty(program)) throw new JsonException($"'{ProgramDiscriminator}' field is null or empty.");

        return program switch
        {
            "Average2SavageRepsPerSet" => DeserializeRepsPerSet(root, options),
            "Average2SavageHypertrophy" => DeserializeLinearProgression(root, options),
            _ => throw new JsonException($"Unknown '{ProgramDiscriminator}' value: {program}")
        };
    }

    private static RepsPerSetExerciseDetailsRequest DeserializeRepsPerSet(JsonElement root,
        JsonSerializerOptions options)
    {
        return new RepsPerSetExerciseDetailsRequest(
            root.GetProperty("MinimumReps").GetInt32(),
            root.GetProperty("TargetReps").GetInt32(),
            root.GetProperty("MaximumTargetReps").GetInt32(),
            root.GetProperty("NumberOfSets").GetInt32(),
            root.GetProperty("TotalNumberOfSets").GetInt32(),
            ExerciseProgram.Average2SavageRepsPerSet // Hardcoded to ensure consistency
        );
    }

    private static LinearProgressionExerciseDetailsRequest DeserializeLinearProgression(JsonElement root,
        JsonSerializerOptions options)
    {
        return new LinearProgressionExerciseDetailsRequest(
            root.GetProperty("WeightProgression").GetDecimal(),
            root.GetProperty("AttemptsBeforeDeload").GetInt32(),
            ExerciseProgram.Average2SavageHypertrophy, // Hardcoded to ensure consistency
            Enum.Parse<BodyCategory>(root.GetProperty("BodyCategory").GetString() ?? string.Empty),
            Enum.Parse<EquipmentType>(root.GetProperty("EquipmentType").GetString() ?? string.Empty)
        );
    }

    public override void Write(Utf8JsonWriter writer, ExerciseDetailsRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        // Write the discriminator field
        writer.WriteString(ProgramDiscriminator, value is RepsPerSetExerciseDetailsRequest
            ? nameof(ExerciseProgram.Average2SavageRepsPerSet)
            : nameof(ExerciseProgram.Average2SavageHypertrophy));

        // Write fields based on the specific type of ExerciseDetailsRequest
        switch (value)
        {
            case RepsPerSetExerciseDetailsRequest reps:
                writer.WriteNumber(nameof(reps.MinimumReps), reps.MinimumReps);
                writer.WriteNumber(nameof(reps.TargetReps), reps.TargetReps);
                writer.WriteNumber(nameof(reps.MaximumTargetReps), reps.MaximumTargetReps);
                writer.WriteNumber(nameof(reps.NumberOfSets), reps.NumberOfSets);
                writer.WriteNumber(nameof(reps.TotalNumberOfSets), reps.TotalNumberOfSets);
                break;

            case LinearProgressionExerciseDetailsRequest linear:
                writer.WriteNumber(nameof(linear.WeightProgression), (double)linear.WeightProgression);
                writer.WriteNumber(nameof(linear.AttemptsBeforeDeload), linear.AttemptsBeforeDeload);
                writer.WriteString(nameof(linear.BodyCategory), linear.BodyCategory.ToString());
                writer.WriteString(nameof(linear.EquipmentType), linear.EquipmentType.ToString());
                break;

            default:
                throw new JsonException($"Unknown ExerciseDetailsRequest type: {value.GetType()}");
        }

        writer.WriteEndObject();
    }
}