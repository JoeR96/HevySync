using System.Text.Json;
using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Responses;

namespace HevySync.Endpoints.Average2Savage.Converters;

public class ExerciseDetailDtoConverter : JsonConverter<ExerciseDetailDto>
{
    private const string ProgramDiscriminator = "Program";

    public override ExerciseDetailDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (!root.TryGetProperty(ProgramDiscriminator, out var typeElement))
            throw new JsonException(
                $"Missing '{ProgramDiscriminator}' field in JSON payload for polymorphic deserialization.");

        var type = typeElement.GetString();
        if (string.IsNullOrEmpty(type)) throw new JsonException($"'{ProgramDiscriminator}' field is null or empty.");

        return type switch
        {
            nameof(LinearProgressionDto) => DeserializeLinearProgression(root, options),
            nameof(RepsPerSetDto) => DeserializeRepsPerSet(root, options),
            _ => throw new JsonException($"Unknown '{ProgramDiscriminator}' value: {type}")
        };
    }

    private static LinearProgressionDto DeserializeLinearProgression(JsonElement root, JsonSerializerOptions options)
    {
        return new LinearProgressionDto
        {
            Id = root.GetProperty(nameof(LinearProgressionDto.Id)).GetGuid(),
            WeightProgression = root.GetProperty(nameof(LinearProgressionDto.WeightProgression)).GetDecimal(),
            AttemptsBeforeDeload = root.GetProperty(nameof(LinearProgressionDto.AttemptsBeforeDeload)).GetInt32(),
            TrainingMax = root.GetProperty(nameof(LinearProgressionDto.TrainingMax)).GetDecimal()
        };
    }

    private static RepsPerSetDto DeserializeRepsPerSet(JsonElement root, JsonSerializerOptions options)
    {
        return new RepsPerSetDto
        {
            Id = root.GetProperty(nameof(RepsPerSetDto.Id)).GetGuid(),
            MinimumReps = root.GetProperty(nameof(RepsPerSetDto.MinimumReps)).GetInt32(),
            TargetReps = root.GetProperty(nameof(RepsPerSetDto.TargetReps)).GetInt32(),
            MaximumTargetReps = root.GetProperty(nameof(RepsPerSetDto.MaximumTargetReps)).GetInt32(),
            StartingSetCount = root.GetProperty(nameof(RepsPerSetDto.StartingSetCount)).GetInt32(),
            TargetSetCount = root.GetProperty(nameof(RepsPerSetDto.TargetSetCount)).GetInt32()
        };
    }

    public override void Write(Utf8JsonWriter writer, ExerciseDetailDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(ProgramDiscriminator, value.GetType().Name);

        switch (value)
        {
            case LinearProgressionDto linear:
                writer.WriteString(nameof(linear.Id), linear.Id.ToString());
                writer.WriteNumber(nameof(linear.WeightProgression), (double)linear.WeightProgression);
                writer.WriteNumber(nameof(linear.AttemptsBeforeDeload), linear.AttemptsBeforeDeload);
                writer.WriteNumber(nameof(linear.TrainingMax), (double)linear.TrainingMax);
                break;

            case RepsPerSetDto reps:
                writer.WriteString(nameof(reps.Id), reps.Id.ToString());
                writer.WriteNumber(nameof(reps.MinimumReps), reps.MinimumReps);
                writer.WriteNumber(nameof(reps.TargetReps), reps.TargetReps);
                writer.WriteNumber(nameof(reps.MaximumTargetReps), reps.MaximumTargetReps);
                writer.WriteNumber(nameof(reps.StartingSetCount), reps.StartingSetCount);
                writer.WriteNumber(nameof(reps.TargetSetCount), reps.TargetSetCount);
                break;

            default:
                throw new JsonException($"Unknown ExerciseDetailDto type: {value.GetType()}");
        }

        writer.WriteEndObject();
    }
}