using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HevySync.Endpoints.Average2Savage.Converters;

namespace HevySync.IntegrationTests.Extensions;

public static class HttpExtensions
{
    public static async Task<T?> GetAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(GetDefaultJsonSerializerOptions());
    }

    public static async Task<T?> PostAsync<T>(this HttpClient client, string requestUri, object? data)
    {
        var response = await client.PostAsJsonAsync(requestUri, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(GetDefaultJsonSerializerOptions());
    }

    public static async Task<T?> PutAsync<T>(this HttpClient client, string requestUri, object? data)
    {
        var response = await client.PutAsJsonAsync(requestUri, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(GetDefaultJsonSerializerOptions());
    }

    public static async Task<T?> DeleteAsync<T>(this HttpClient client, string requestUri)
    {
        var response = await client.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(GetDefaultJsonSerializerOptions());
    }

    private static JsonSerializerOptions GetDefaultJsonSerializerOptions()
    {
        var defaultJsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        defaultJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        defaultJsonSerializerOptions.Converters.Add(new ExerciseDetailsRequestConverter());
        defaultJsonSerializerOptions.Converters.Add(new ExerciseDetailDtoConverter());
        return defaultJsonSerializerOptions;
    }
}