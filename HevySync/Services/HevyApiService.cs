namespace HevySync.Services;

public class HevyApiService
{
    private readonly HttpClient _httpClient;

    public HevyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
 
    public async Task<string> GetWorkoutsAsync(int page = 1, int pageSize = 5)
    {
        var response = await _httpClient.GetAsync($"workouts?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}