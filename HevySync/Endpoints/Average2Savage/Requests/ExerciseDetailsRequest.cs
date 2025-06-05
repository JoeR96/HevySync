using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Converters;

namespace HevySync.Endpoints.Average2Savage.Requests;

[JsonConverter(typeof(ExerciseDetailsRequestConverter))]
public abstract record ExerciseDetailsRequest;