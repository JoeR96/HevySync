using System.Text.Json;
using HevySync.Models;

namespace HevySync.Services;

public class HevyApiService(HttpClient httpClient)
{
    public async Task<HevyApiResponse> GetWorkoutEventsAsync(
        DateTimeOffset lastWorkoutDateTimeOffset,
        int page = 1,
        int pageSize = 5)
    {
        var since = lastWorkoutDateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ"); // Example format
        var response = await httpClient.GetAsync($"workouts/events?page={page}&pageSize={pageSize}&since={since}");
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var hevyApiResponse = JsonSerializer.Deserialize<HevyApiResponse>(
            jsonResponse,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return hevyApiResponse;
    }
}