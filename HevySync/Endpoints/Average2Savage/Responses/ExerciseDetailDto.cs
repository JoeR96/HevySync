using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Converters;

namespace HevySync.Endpoints.Average2Savage.Responses;

[JsonConverter(typeof(ExerciseDetailDtoConverter))]
public record ExerciseDetailDto
{
    public Guid Id { get; set; }
}