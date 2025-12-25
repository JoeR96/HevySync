# Hevy App Integration Plan

**Purpose**: Design and implementation plan for integrating HevySync with the Hevy mobile app

**Status**: Not Started

**Priority**: Critical (Core Product Feature)

**Estimated Effort**: 12-16 hours

---

## Table of Contents

1. [Overview](#overview)
2. [Hevy API Understanding](#hevy-api-understanding)
3. [Integration Architecture](#integration-architecture)
4. [Implementation Plan](#implementation-plan)
5. [Domain Model Changes](#domain-model-changes)
6. [API Client Design](#api-client-design)
7. [Exercise Mapping Strategy](#exercise-mapping-strategy)
8. [Event-Driven Sync](#event-driven-sync)
9. [Error Handling & Retry Logic](#error-handling--retry-logic)
10. [Testing Strategy](#testing-strategy)

---

## Overview

### Goal

Enable HevySync to create workouts in the Hevy mobile app, so users can:
1. Configure A2S program in HevySync web app
2. Generate weekly workouts server-side using A2S formulas
3. **Automatically sync workouts to Hevy app**
4. Execute workouts in Hevy mobile app
5. Track performance in Hevy
6. Complete workout in HevySync to trigger progression

### User Flow

```
User creates A2S workout in HevySync
              │
              ▼
HevySync generates Week 1 sets
              │
              ▼
HevySync creates workout in Hevy app ◄─── NEW FEATURE
              │
              ▼
User opens Hevy mobile app
              │
              ▼
Workout appears in Hevy with all exercises/sets
              │
              ▼
User completes workout in Hevy
              │
              ▼
User records completion in HevySync
              │
              ▼
HevySync applies A2S progression
              │
              ▼
HevySync updates next week in Hevy app
```

---

## Hevy API Understanding

### Authentication

**Method**: API Key or OAuth

**Required Information**:
- Hevy account credentials
- API key from Hevy developer portal
- User authorization flow

**Storage**:
- User table: `HevyApiKey` encrypted column
- User table: `HevyUserId` for mapping

### Hevy API Endpoints (Assumed)

**Note**: These are assumed based on typical fitness app APIs. Actual Hevy API documentation needed.

```
POST /api/v1/workouts
GET /api/v1/workouts/{id}
PUT /api/v1/workouts/{id}
DELETE /api/v1/workouts/{id}

GET /api/v1/exercises
GET /api/v1/exercises/{id}
POST /api/v1/exercises/search

POST /api/v1/workouts/{workout_id}/routines
PUT /api/v1/workouts/{workout_id}/routines/{routine_id}
```

### Hevy Workout Data Model

```json
{
  "id": "hevy_workout_123",
  "title": "A2S Hypertrophy - Week 1, Day 1",
  "description": "Average to Savage program",
  "folder_id": null,
  "exercises": [
    {
      "id": "hevy_exercise_456",
      "exercise_template_id": "squat_template_789",
      "title": "Squat",
      "exercise_order": 1,
      "rest_seconds": 180,
      "sets": [
        {
          "set_index": 0,
          "weight_kg": 81.6,
          "reps": 6,
          "rpe": null,
          "completed": false
        },
        {
          "set_index": 1,
          "weight_kg": 81.6,
          "reps": 6,
          "rpe": null,
          "completed": false
        },
        {
          "set_index": 2,
          "weight_kg": 81.6,
          "reps": "6+",
          "rpe": null,
          "completed": false,
          "is_amrap": true
        }
      ]
    }
  ]
}
```

### Hevy Exercise Templates

Hevy has predefined exercise templates (Squat, Bench Press, etc.).

**Challenge**: Map A2S exercise names to Hevy template IDs.

**Options**:
1. Manual mapping table (user selects Hevy exercise for each A2S exercise)
2. Fuzzy name matching (automatic suggestion)
3. Hevy search API (search by name)

---

## Integration Architecture

### Bounded Context: Hevy Integration

Create a separate bounded context for Hevy integration to maintain separation of concerns.

```
┌─────────────────────────────────────────────────────────────────┐
│              HEVY INTEGRATION BOUNDED CONTEXT                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  Aggregates:                                                     │
│  - HevyWorkoutSync (tracks sync status)                          │
│  - ExerciseMapping (maps A2S exercises to Hevy templates)        │
│                                                                  │
│  Domain Services:                                                │
│  - IHevyApiClient (HTTP client to Hevy API)                      │
│  - IExerciseMappingService (resolve exercise mappings)           │
│  - IWorkoutSyncService (orchestrate sync operations)             │
│                                                                  │
│  Events Consumed:                                                │
│  - WorkoutCreatedEvent                                           │
│  - WorkoutDayCompletedEvent (if Hevy needs updates)              │
│                                                                  │
│  Events Published:                                               │
│  - HevyWorkoutSyncedEvent                                        │
│  - HevyWorkoutSyncFailedEvent                                    │
│                                                                  │
│  Anti-Corruption Layer (ACL):                                    │
│  - HevyWorkoutMapper (domain Workout → Hevy API format)          │
│  - HevyExerciseMapper (domain Exercise → Hevy exercise format)   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### System Context Diagram

```
┌──────────────┐                           ┌──────────────┐
│              │  WorkoutCreatedEvent       │              │
│   Workout    ├──────────────────────────►│    Hevy      │
│   Management │                           │  Integration │
│   Context    │  HevyWorkoutSyncedEvent   │   Context    │
│              │◄──────────────────────────┤              │
└──────────────┘                           └──────┬───────┘
                                                  │
                                                  │ HTTP/REST
                                                  │
                                                  ▼
                                          ┌──────────────┐
                                          │              │
                                          │  Hevy API    │
                                          │  (External)  │
                                          │              │
                                          └──────────────┘
```

---

## Implementation Plan

### Phase 1: Domain Model Changes (2 hours)

#### Task 1.1: Add Hevy Sync Tracking to Workout Aggregate

```csharp
public class Workout : AggregateRoot<Guid>
{
    // Existing properties...

    // New properties for Hevy sync
    public string? HevyWorkoutId { get; private set; }
    public HevySyncStatus SyncStatus { get; private set; } = HevySyncStatus.NotSynced;
    public DateTime? LastSyncedAt { get; private set; }
    public string? SyncErrorMessage { get; private set; }

    // New methods
    public void MarkSyncedWithHevy(string hevyWorkoutId)
    {
        if (string.IsNullOrWhiteSpace(hevyWorkoutId))
            throw new DomainException("Hevy workout ID cannot be empty");

        HevyWorkoutId = hevyWorkoutId;
        SyncStatus = HevySyncStatus.Synced;
        LastSyncedAt = DateTime.UtcNow;
        SyncErrorMessage = null;

        AddDomainEvent(new HevyWorkoutSyncedEvent(Id, hevyWorkoutId, DateTime.UtcNow));
    }

    public void MarkSyncInProgress()
    {
        SyncStatus = HevySyncStatus.Syncing;
        SyncErrorMessage = null;
    }

    public void MarkSyncFailed(string errorMessage)
    {
        SyncStatus = HevySyncStatus.Failed;
        SyncErrorMessage = errorMessage;

        AddDomainEvent(new HevyWorkoutSyncFailedEvent(Id, errorMessage, DateTime.UtcNow));
    }
}

public enum HevySyncStatus
{
    NotSynced,
    Syncing,
    Synced,
    Failed
}
```

#### Task 1.2: Create Hevy Domain Events

```csharp
// HevySync.Domain/Events/HevyWorkoutSyncedEvent.cs
public record HevyWorkoutSyncedEvent(
    Guid WorkoutId,
    string HevyWorkoutId,
    DateTime OccurredOn
) : IDomainEvent;

// HevySync.Domain/Events/HevyWorkoutSyncFailedEvent.cs
public record HevyWorkoutSyncFailedEvent(
    Guid WorkoutId,
    string ErrorMessage,
    DateTime OccurredOn
) : IDomainEvent;
```

#### Task 1.3: Create Exercise Mapping Aggregate

```csharp
// HevySync.Domain/Aggregates/ExerciseMapping.cs
public class ExerciseMapping : AggregateRoot<Guid>
{
    private ExerciseMapping() { }

    public Guid UserId { get; private set; }
    public string ExerciseName { get; private set; } // A2S exercise name
    public string HevyTemplateId { get; private set; } // Hevy exercise template ID
    public string HevyTemplateName { get; private set; } // For display
    public bool IsVerified { get; private set; } // User confirmed mapping
    public DateTime CreatedAt { get; private set; }

    public static ExerciseMapping Create(
        Guid userId,
        string exerciseName,
        string hevyTemplateId,
        string hevyTemplateName,
        bool isVerified = false)
    {
        return new ExerciseMapping
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseName = exerciseName.Trim().ToLowerInvariant(),
            HevyTemplateId = hevyTemplateId,
            HevyTemplateName = hevyTemplateName,
            IsVerified = isVerified,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void VerifyMapping()
    {
        IsVerified = true;
    }
}
```

---

### Phase 2: Infrastructure - API Client (4 hours)

#### Task 2.1: Create Hevy API Models

```csharp
// HevySync.Infrastructure/Hevy/Models/HevyWorkoutRequest.cs
namespace HevySync.Infrastructure.Hevy.Models;

public record HevyWorkoutRequest
{
    public string Title { get; init; }
    public string? Description { get; init; }
    public List<HevyExerciseRequest> Exercises { get; init; }
}

public record HevyExerciseRequest
{
    public string ExerciseTemplateId { get; init; }
    public string Title { get; init; }
    public int ExerciseOrder { get; init; }
    public int RestSeconds { get; init; }
    public List<HevySetRequest> Sets { get; init; }
}

public record HevySetRequest
{
    public int SetIndex { get; init; }
    public decimal? WeightKg { get; init; }
    public object Reps { get; init; } // Can be int or string ("6+")
    public int? Rpe { get; init; }
    public bool IsAmrap { get; init; }
}

// HevySync.Infrastructure/Hevy/Models/HevyWorkoutResponse.cs
public record HevyWorkoutResponse
{
    public string Id { get; init; }
    public string Title { get; init; }
    public DateTime CreatedAt { get; init; }
}

// HevySync.Infrastructure/Hevy/Models/HevyExerciseTemplate.cs
public record HevyExerciseTemplate
{
    public string Id { get; init; }
    public string Title { get; init; }
    public string Category { get; init; }
    public string Equipment { get; init; }
}
```

#### Task 2.2: Implement Hevy API Client Interface

```csharp
// HevySync.Domain/DomainServices/IHevyApiClient.cs
namespace HevySync.Domain.DomainServices;

public interface IHevyApiClient
{
    /// <summary>
    /// Creates a workout in Hevy app.
    /// </summary>
    Task<HevyWorkoutResponse> CreateWorkoutAsync(
        HevyWorkoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing Hevy workout.
    /// </summary>
    Task<HevyWorkoutResponse> UpdateWorkoutAsync(
        string hevyWorkoutId,
        HevyWorkoutRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Hevy workout.
    /// </summary>
    Task DeleteWorkoutAsync(
        string hevyWorkoutId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for exercise templates by name.
    /// </summary>
    Task<List<HevyExerciseTemplate>> SearchExercisesAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available exercise templates.
    /// </summary>
    Task<List<HevyExerciseTemplate>> GetExerciseTemplatesAsync(
        CancellationToken cancellationToken = default);
}
```

#### Task 2.3: Implement Hevy API Client

```csharp
// HevySync.Infrastructure/Hevy/HevyApiClient.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace HevySync.Infrastructure.Hevy;

public class HevyApiClient : IHevyApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HevyApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HevyApiClient(
        HttpClient httpClient,
        ILogger<HevyApiClient> logger,
        IOptions<HevyApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Configure base address and headers
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", options.Value.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    public async Task<HevyWorkoutResponse> CreateWorkoutAsync(
        HevyWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating workout in Hevy: {Title}", request.Title);

            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/workouts",
                request,
                _jsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<HevyWorkoutResponse>(
                _jsonOptions,
                cancellationToken);

            _logger.LogInformation("Successfully created Hevy workout: {HevyWorkoutId}", result?.Id);

            return result ?? throw new HevyApiException("Empty response from Hevy API");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error creating Hevy workout");
            throw new HevyApiException("Failed to create workout in Hevy", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Hevy workout");
            throw new HevyApiException("Unexpected error creating workout in Hevy", ex);
        }
    }

    public async Task<HevyWorkoutResponse> UpdateWorkoutAsync(
        string hevyWorkoutId,
        HevyWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating Hevy workout: {HevyWorkoutId}", hevyWorkoutId);

            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/workouts/{hevyWorkoutId}",
                request,
                _jsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<HevyWorkoutResponse>(
                _jsonOptions,
                cancellationToken);

            return result ?? throw new HevyApiException("Empty response from Hevy API");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Hevy workout {HevyWorkoutId}", hevyWorkoutId);
            throw new HevyApiException($"Failed to update Hevy workout {hevyWorkoutId}", ex);
        }
    }

    public async Task DeleteWorkoutAsync(
        string hevyWorkoutId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/workouts/{hevyWorkoutId}",
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Hevy workout {HevyWorkoutId}", hevyWorkoutId);
            throw new HevyApiException($"Failed to delete Hevy workout {hevyWorkoutId}", ex);
        }
    }

    public async Task<List<HevyExerciseTemplate>> SearchExercisesAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"api/v1/exercises?search={Uri.EscapeDataString(searchTerm)}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<HevyExerciseTemplate>>(
                _jsonOptions,
                cancellationToken);

            return results ?? new List<HevyExerciseTemplate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Hevy exercises: {SearchTerm}", searchTerm);
            return new List<HevyExerciseTemplate>();
        }
    }

    public async Task<List<HevyExerciseTemplate>> GetExerciseTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/exercises", cancellationToken);
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<List<HevyExerciseTemplate>>(
                _jsonOptions,
                cancellationToken);

            return results ?? new List<HevyExerciseTemplate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Hevy exercise templates");
            throw new HevyApiException("Failed to fetch exercise templates", ex);
        }
    }
}

// HevySync.Infrastructure/Hevy/HevyApiOptions.cs
public class HevyApiOptions
{
    public string BaseUrl { get; set; } = "https://api.hevyapp.com";
    public string ApiKey { get; set; } = "";
}

// HevySync.Domain/Exceptions/HevyApiException.cs
public class HevyApiException : Exception
{
    public HevyApiException(string message) : base(message) { }
    public HevyApiException(string message, Exception inner) : base(message, inner) { }
}
```

#### Task 2.4: Register API Client in DI

```csharp
// HevySync/DependencyInjection.cs or Program.cs
services.Configure<HevyApiOptions>(configuration.GetSection("Hevy"));

services.AddHttpClient<IHevyApiClient, HevyApiClient>()
    .ConfigureHttpClient((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<HevyApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            // Log retry attempts
        }
    ));
```

#### Task 2.5: Add Configuration

```json
// appsettings.json
{
  "Hevy": {
    "BaseUrl": "https://api.hevyapp.com",
    "ApiKey": "" // Set via environment variable or user secrets
  }
}
```

---

### Phase 3: Exercise Mapping Service (3 hours)

#### Task 3.1: Create Mapping Service Interface

```csharp
// HevySync.Domain/DomainServices/IExerciseMappingService.cs
public interface IExerciseMappingService
{
    /// <summary>
    /// Gets Hevy template ID for an exercise, or suggests mapping.
    /// </summary>
    Task<string?> GetHevyTemplateIdAsync(
        Guid userId,
        string exerciseName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates exercise mapping.
    /// </summary>
    Task<ExerciseMapping> MapExerciseAsync(
        Guid userId,
        string exerciseName,
        string hevyTemplateId,
        string hevyTemplateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests Hevy exercises based on A2S exercise name.
    /// </summary>
    Task<List<HevyExerciseTemplate>> SuggestMappingsAsync(
        string exerciseName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user mappings.
    /// </summary>
    Task<List<ExerciseMapping>> GetUserMappingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
```

#### Task 3.2: Implement Mapping Service

```csharp
// HevySync.Infrastructure/DomainServices/ExerciseMappingService.cs
public class ExerciseMappingService : IExerciseMappingService
{
    private readonly IExerciseMappingRepository _repository;
    private readonly IHevyApiClient _hevyClient;
    private readonly ILogger<ExerciseMappingService> _logger;

    // Fallback mapping for common exercises
    private static readonly Dictionary<string, string> DefaultMappings = new()
    {
        { "squat", "squat_barbell" },
        { "back squat", "squat_barbell" },
        { "bench press", "bench_press_barbell" },
        { "deadlift", "deadlift_barbell" },
        { "overhead press", "overhead_press_barbell" },
        { "barbell row", "row_barbell" },
        { "pull up", "pull_up" },
        { "chin up", "chin_up" }
    };

    public async Task<string?> GetHevyTemplateIdAsync(
        Guid userId,
        string exerciseName,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = exerciseName.Trim().ToLowerInvariant();

        // Check if user has mapped this exercise
        var mapping = await _repository.GetByUserAndExerciseNameAsync(
            userId,
            normalizedName,
            cancellationToken);

        if (mapping != null)
            return mapping.HevyTemplateId;

        // Check default mappings
        if (DefaultMappings.TryGetValue(normalizedName, out var defaultTemplate))
            return defaultTemplate;

        // No mapping found
        _logger.LogWarning("No Hevy mapping found for exercise: {ExerciseName}", exerciseName);
        return null;
    }

    public async Task<List<HevyExerciseTemplate>> SuggestMappingsAsync(
        string exerciseName,
        CancellationToken cancellationToken = default)
    {
        // Search Hevy API for similar exercises
        var results = await _hevyClient.SearchExercisesAsync(exerciseName, cancellationToken);

        // Apply fuzzy matching to improve suggestions
        var scored = results
            .Select(template => new
            {
                Template = template,
                Score = CalculateSimilarity(exerciseName, template.Title)
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Template)
            .ToList();

        return scored;
    }

    private double CalculateSimilarity(string source, string target)
    {
        // Simple Levenshtein distance or use FuzzySharp library
        // For now, basic contains check
        source = source.ToLowerInvariant();
        target = target.ToLowerInvariant();

        if (source == target) return 1.0;
        if (target.Contains(source)) return 0.8;
        if (source.Contains(target)) return 0.6;

        return 0.0;
    }

    // Other methods...
}
```

---

### Phase 4: Event Handlers for Sync (3 hours)

#### Task 4.1: Create Workout Created Event Handler

```csharp
// HevySync.Application/Workouts/EventHandlers/WorkoutCreatedEventHandler.cs
public class WorkoutCreatedEventHandler : INotificationHandler<WorkoutCreatedEvent>
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IHevyApiClient _hevyClient;
    private readonly IExerciseMappingService _mappingService;
    private readonly IHevyWorkoutMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkoutCreatedEventHandler> _logger;

    public async Task Handle(
        WorkoutCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Handling WorkoutCreatedEvent for workout: {WorkoutId}",
                notification.WorkoutId);

            // Load workout with exercises
            var workout = await _workoutRepository.GetByIdWithExercisesAsync(
                notification.WorkoutId,
                cancellationToken);

            if (workout == null)
            {
                _logger.LogError("Workout not found: {WorkoutId}", notification.WorkoutId);
                return;
            }

            // Mark sync in progress
            workout.MarkSyncInProgress();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Validate all exercises have Hevy mappings
            await EnsureExerciseMappingsAsync(workout, cancellationToken);

            // Map domain workout to Hevy API format
            var hevyRequest = await _mapper.MapToHevyWorkoutAsync(
                workout,
                cancellationToken);

            // Create workout in Hevy
            var hevyResponse = await _hevyClient.CreateWorkoutAsync(
                hevyRequest,
                cancellationToken);

            // Mark sync complete
            workout.MarkSyncedWithHevy(hevyResponse.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully synced workout {WorkoutId} to Hevy: {HevyWorkoutId}",
                notification.WorkoutId,
                hevyResponse.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing workout {WorkoutId} to Hevy", notification.WorkoutId);

            // Mark sync failed
            var workout = await _workoutRepository.GetByIdAsync(
                notification.WorkoutId,
                cancellationToken);

            workout?.MarkSyncFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureExerciseMappingsAsync(
        Workout workout,
        CancellationToken cancellationToken)
    {
        foreach (var exercise in workout.Exercises)
        {
            var hevyTemplateId = await _mappingService.GetHevyTemplateIdAsync(
                workout.UserId,
                exercise.Name.Value,
                cancellationToken);

            if (string.IsNullOrEmpty(hevyTemplateId))
            {
                throw new HevyApiException(
                    $"No Hevy mapping found for exercise: {exercise.Name.Value}. " +
                    "Please map exercises before syncing.");
            }

            // Update exercise with template ID if not set
            if (string.IsNullOrEmpty(exercise.ExerciseTemplateId))
            {
                exercise.MapToHevyTemplate(hevyTemplateId);
            }
        }
    }
}
```

#### Task 4.2: Create Hevy Workout Mapper

```csharp
// HevySync.Infrastructure/Hevy/Mappers/HevyWorkoutMapper.cs
public interface IHevyWorkoutMapper
{
    Task<HevyWorkoutRequest> MapToHevyWorkoutAsync(
        Workout workout,
        CancellationToken cancellationToken = default);
}

public class HevyWorkoutMapper : IHevyWorkoutMapper
{
    private readonly ISetGenerationService _setGenerationService;
    private readonly IExerciseMappingService _mappingService;

    public async Task<HevyWorkoutRequest> MapToHevyWorkoutAsync(
        Workout workout,
        CancellationToken cancellationToken = default)
    {
        var exercises = new List<HevyExerciseRequest>();

        // Generate sets for Week 1, Day 1 (or current week/day)
        var exercisesForDay = workout.Exercises
            .Where(e => e.Day == 1) // Start with Day 1
            .OrderBy(e => e.Order)
            .ToList();

        foreach (var exercise in exercisesForDay)
        {
            var sets = await _setGenerationService.GenerateSetsAsync(
                exercise,
                workout.Activity.Week,
                cancellationToken);

            var hevyExercise = new HevyExerciseRequest
            {
                ExerciseTemplateId = exercise.ExerciseTemplateId!,
                Title = exercise.Name.Value,
                ExerciseOrder = exercise.Order,
                RestSeconds = exercise.RestTimer.Seconds,
                Sets = sets.Select((set, index) => new HevySetRequest
                {
                    SetIndex = index,
                    WeightKg = ConvertToKg(set.Weight),
                    Reps = set.IsAmrap ? $"{set.Reps}+" : set.Reps,
                    IsAmrap = set.IsAmrap,
                    Rpe = null
                }).ToList()
            };

            exercises.Add(hevyExercise);
        }

        return new HevyWorkoutRequest
        {
            Title = $"{workout.Name.Value} - Week {workout.Activity.Week}, Day 1",
            Description = "Generated by HevySync - Average to Savage program",
            Exercises = exercises
        };
    }

    private decimal ConvertToKg(decimal pounds)
    {
        // TODO: Use user preference for units
        return Math.Round(pounds * 0.453592m, 2);
    }
}
```

---

### Phase 5: API Endpoints for Exercise Mapping (2-3 hours)

#### Task 5.1: Create Exercise Mapping Endpoints

```csharp
// HevySync/Endpoints/Hevy/MapExercise.cs
public class MapExercise : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/hevy/exercise-mappings", HandleAsync)
            .WithTags("Hevy")
            .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(
        MapExerciseRequest request,
        IExerciseMappingService mappingService,
        CancellationToken cancellationToken)
    {
        var mapping = await mappingService.MapExerciseAsync(
            request.UserId,
            request.ExerciseName,
            request.HevyTemplateId,
            request.HevyTemplateName,
            cancellationToken);

        return Results.Ok(mapping);
    }
}

// HevySync/Endpoints/Hevy/SuggestExerciseMapping.cs
public class SuggestExerciseMapping : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/hevy/exercise-mappings/suggest", HandleAsync)
            .WithTags("Hevy")
            .RequireAuthorization();
    }

    public async Task<IResult> HandleAsync(
        [FromQuery] string exerciseName,
        IExerciseMappingService mappingService,
        CancellationToken cancellationToken)
    {
        var suggestions = await mappingService.SuggestMappingsAsync(
            exerciseName,
            cancellationToken);

        return Results.Ok(suggestions);
    }
}
```

---

## Summary

### Implementation Phases

| Phase | Description | Effort | Priority |
|-------|-------------|--------|----------|
| 1 | Domain model changes | 2 hours | Critical |
| 2 | API client implementation | 4 hours | Critical |
| 3 | Exercise mapping service | 3 hours | High |
| 4 | Event-driven sync | 3 hours | Critical |
| 5 | API endpoints for mapping | 2-3 hours | Medium |

**Total Estimated Effort**: 14-15 hours

### Next Steps

1. ✅ Review this integration plan
2. ⬜ Obtain Hevy API documentation (if available)
3. ⬜ Set up Hevy developer account and API keys
4. ⬜ Implement Phase 1: Domain changes
5. ⬜ Implement Phase 2: API client
6. ⬜ Test Hevy API integration with Postman/curl
7. ⬜ Implement Phase 3: Exercise mapping
8. ⬜ Implement Phase 4: Event handlers
9. ⬜ End-to-end testing

### Open Questions

1. **Does Hevy have a public API?**
   - If not, may need to use web scraping or reverse engineering (legal considerations)
   - Alternative: Partner with Hevy or use webhooks

2. **Authentication method?**
   - API key per user?
   - OAuth flow?
   - How to securely store credentials?

3. **Rate limiting?**
   - What are Hevy's API rate limits?
   - Do we need request queuing/throttling?

4. **Workout updates vs. new workouts?**
   - Should we update existing Hevy workout for next week?
   - Or create new workout for each week?

5. **Conflict resolution?**
   - What if user modifies workout in Hevy?
   - Do we sync changes back to HevySync?

