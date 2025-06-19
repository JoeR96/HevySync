using System.Text.Json;
using System.Text.Json.Serialization;
using HevySync.Models.Hevy;

namespace HevySync.Services;

public class HevyApiService(HttpClient httpClient)
{
    public async Task<HevyApiResponse> GetWorkoutEventsAsync(
        DateTimeOffset lastWorkoutDateTimeOffset,
        int page = 1,
        int pageSize = 5)
    {
        var since = lastWorkoutDateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ssZ");
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

    public async Task<List<string>> GetAllExerciseTemplateIdsAsync()
    {
        var exerciseTemplateIds = new List<string>();


        var currentPage = 1;
        var morePagesAvailable = true;

        while (morePagesAvailable)
        {
            // Construct API URL with pagination
            var url = $"exercise_templates?page={currentPage}";

            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Read the response body as a string
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response to extract IDs
                var responseData = JsonSerializer.Deserialize<AllExerciseTemplatesResponse>(responseBody);

                // Add all IDs from the current page to the list
                foreach (var template in responseData.ExerciseTemplates) exerciseTemplateIds.Add(template.Id);

                // Check if there are more pages to process
                currentPage++;
                morePagesAvailable = currentPage <= responseData.PageCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching templates from API: {ex.Message}");
                break;
            }
        }

        Console.WriteLine($"Total exercise_template_ids fetched: {exerciseTemplateIds.Count}");
        return exerciseTemplateIds;
    }

    public async Task<RoutineResponse> CreateRoutineAsync(RoutineRequest routineRequest)
    {
        var templates = await GetAllExerciseTemplateIdsAsync();

        routineRequest.Routine.Exercises.ForEach(e =>
            e.ExerciseTemplateId = templates[Random.Shared.Next(0, templates.Count - 1)]);

        var response = await httpClient.PostAsJsonAsync("routines", routineRequest);

        // 1f9ebf03-5439-460a-a93c-bc4e877ae2b9
        var _ = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var routineResponse = JsonSerializer.Deserialize<RoutineResponse>(
            jsonResponse,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return routineResponse;
    }

    public async Task<RoutineFoldersResponse> GetRoutineFoldersAsync(int page = 1, int pageSize = 10)

    {
        //https://api.hevyapp.com/v1/routine_folders?page=1&pageSize=5
        var response =
            await httpClient.GetFromJsonAsync<RoutineFoldersResponse>(
                $"routine_folders?page={page}&pageSize={pageSize}");


        return response;
    }
}

public class RoutineFoldersResponse
{
    [JsonPropertyName("page")] public int Page { get; set; } // Current page number

    [JsonPropertyName("page_count")] public int PageCount { get; set; } // Total number of pages

    [JsonPropertyName("routine_folders")] public List<RoutineFolder> RoutineFolders { get; set; } // List of folders
}

public class RoutineFolder
{
    [JsonPropertyName("id")] public int Id { get; set; } // Example: 1171079

    [JsonPropertyName("index")] public int Index { get; set; } // The index of the folder

    [JsonPropertyName("title")] public string Title { get; set; } // Example: "Average2Savage"

    [JsonPropertyName("updated_at")] public DateTime UpdatedAt { get; set; } // When it was last updated

    [JsonPropertyName("created_at")] public DateTime CreatedAt { get; set; } // When it was created
}

public class RoutineRequest
{
    public Routine Routine { get; set; }
}

public class Routine
{
    [JsonPropertyName("title")] public string Title { get; set; } // Example: "April Leg Day 🔥"

    [JsonPropertyName("folder_id")] public int? FolderId { get; set; } // Nullable

    [JsonPropertyName("notes")]
    public string Notes { get; set; } // Example: "Focus on form over weight. Remember to stretch."

    [JsonPropertyName("exercises")] public List<RoutineExercise> Exercises { get; set; }
}

public class RoutineExercise
{
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } // Example: "D04AC939"

    [JsonPropertyName("superset_id")] public string? SupersetId { get; set; } // Nullable

    [JsonPropertyName("rest_seconds")] public int RestSeconds { get; set; } // Example: 90

    [JsonPropertyName("notes")] public string Notes { get; set; } // Example: "Stay slow and controlled."

    [JsonPropertyName("sets")] public List<RoutineSet> Sets { get; set; }
}

public class RoutineSet
{
    [JsonPropertyName("type")] public string Type { get; set; } = "normal";

    [JsonPropertyName("weight_kg")] public double? WeightKg { get; set; } // Nullable

    [JsonPropertyName("reps")] public int? Reps { get; set; } // Nullable

    [JsonPropertyName("distance_meters")] public double? DistanceMeters { get; set; } // Nullable

    [JsonPropertyName("duration_seconds")] public int? DurationSeconds { get; set; } // Nullable

    [JsonPropertyName("custom_metric")] public string? CustomMetric { get; set; } // Nullable
}

public class RoutineResponse
{
    public string Id { get; set; } // Example: "1A2B3C4D"
    public string Message { get; set; } // Example: "Routine created successfully."
}

public class AllExerciseTemplatesResponse
{
    public int Page { get; set; }
    public int PageCount { get; set; }

    [JsonPropertyName("exercise_templates")]
    public List<ExerciseTemplate> ExerciseTemplates { get; set; }
}

public class ExerciseTemplate
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("primary_muscle_group")]
    public string PrimaryMuscleGroup { get; set; }

    [JsonPropertyName("secondary_muscle_groups")]
    public List<string> SecondaryMuscleGroups { get; set; }

    [JsonPropertyName("equipment")] public string Equipment { get; set; }

    [JsonPropertyName("is_custom")] public bool IsCustom { get; set; }
}