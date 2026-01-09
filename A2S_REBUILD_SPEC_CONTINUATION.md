# A2S Architectural Spec - Continuation

## Sections 9-13

## 9. Application Layer Patterns

### 9.1 Command/Query Responsibility Segregation (CQRS)

**Commands** (write operations):
- `CreateWorkoutPlanCommand`
- `CompleteWorkoutDayCommand`
- `ApplyProgressionCommand`

**Queries** (read operations):
- `GetWorkoutPlanQuery`
- `GetCurrentWeekQuery`
- `GetExercisesForDayQuery`

### 9.2 Key Commands and Handlers

#### CreateWorkoutPlanCommand

**Purpose:** Create a new workout plan with exercises

**Flow:**
1. Validate command (via FluentValidation)
2. Map request DTOs to domain value objects
3. Create Exercise entities with appropriate progression strategies
4. Create WorkoutPlan aggregate root
5. Persist to repository
6. Map domain model to response DTO

**Handler Responsibilities:**
- DTO → Domain mapping
- Aggregate creation via factory method
- Repository coordination
- Domain → DTO mapping

#### CompleteWorkoutDayCommand

**Purpose:** Record performance for current day's exercises

**Flow:**
1. Load WorkoutPlan from repository (with exercises)
2. Map performance DTOs to ExercisePerformance value objects
3. Call `workoutPlan.CompleteDay(performances)`
4. Save updated aggregate
5. Return completion result

**Handler Responsibilities:**
- Load aggregate
- DTO → Value Object mapping
- Delegate business logic to aggregate
- Persist changes

#### ApplyProgressionCommand

**Purpose:** Apply weekly progression to all exercises

**Flow:**
1. Load WorkoutPlan from repository
2. Map week's performance data to value objects
3. Call `workoutPlan.ApplyProgression(weekPerformances)`
4. Save updated aggregate
5. Return progression results

**Handler Responsibilities:**
- Load aggregate
- Coordinate progression application
- Persist changes

### 9.3 Key Queries and Handlers

#### GetWorkoutPlanQuery

**Purpose:** Retrieve complete workout plan details

**Returns:** `WorkoutPlanDto` with all exercises and progression data

#### GetCurrentWeekQuery

**Purpose:** Get generated sets for all days in the current week

**Flow:**
1. Load WorkoutPlan
2. For each day (1 to WorkoutsInWeek):
   - Get exercises for that day
   - For each exercise, call `ISetGenerationService.GenerateSetsAsync()`
   - Map to response DTOs
3. Return `WeeklyPlanDto` with all days

**Returns:** Dictionary of Day → List<SessionExerciseDto>

---

## 10. Infrastructure Requirements

### 10.1 Database Schema

**Tables:**

**workout_plans**
```sql
CREATE TABLE workout_plans (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    name VARCHAR(200) NOT NULL,
    current_week INT NOT NULL,
    current_day INT NOT NULL,
    workouts_in_week INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_workout_plans_user_id ON workout_plans(user_id);
```

**exercises**
```sql
CREATE TABLE exercises (
    id UUID PRIMARY KEY,
    workout_plan_id UUID NOT NULL REFERENCES workout_plans(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    exercise_template_id VARCHAR(100) NOT NULL,
    rest_timer_seconds INT NOT NULL,
    day INT NOT NULL,
    "order" INT NOT NULL,
    number_of_sets INT NOT NULL,
    body_category VARCHAR(50),
    equipment_type VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_exercises_workout_plan_id ON exercises(workout_plan_id);
CREATE INDEX idx_exercises_day ON exercises(day);
```

**exercise_progressions (Table Per Hierarchy)**
```sql
CREATE TABLE exercise_progressions (
    id UUID PRIMARY KEY,
    exercise_id UUID NOT NULL REFERENCES exercises(id) ON DELETE CASCADE,
    progression_type VARCHAR(50) NOT NULL,  -- Discriminator

    -- Hypertrophy fields
    training_max DECIMAL(18,2),
    weight_progression DECIMAL(18,2),
    attempts_before_deload INT,
    is_primary BOOLEAN,

    -- RepsPerSet fields
    minimum_reps INT,
    target_reps INT,
    maximum_reps INT,
    starting_set_count INT,
    target_set_count INT,
    starting_weight DECIMAL(18,2),

    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX idx_exercise_progressions_exercise_id
    ON exercise_progressions(exercise_id);
CREATE INDEX idx_exercise_progressions_type
    ON exercise_progressions(progression_type);
```

### 10.2 Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=a2s_tracker;Username=postgres;Password=password"
  },
  "Hypertrophy": {
    "DefaultRoundingValue": 1.25,
    "HypertrophyBlock": {
      "Primary": {
        "Sets": 5,
        "Intensity": [0.70, 0.75, 0.80, 0.725, 0.775, 0.825],
        "RepsPerSet": [5, 4, 3, 5, 4, 3],
        "AmrapRepTarget": [10, 8, 6, 9, 7, 5]
      },
      "Auxiliary": {
        "Sets": 3,
        "Intensity": [0.60, 0.65, 0.70, 0.625, 0.675, 0.725],
        "RepsPerSet": [7, 6, 5, 7, 6, 5],
        "AmrapRepTarget": [14, 12, 10, 13, 11, 9]
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### 10.3 Domain Service Implementations

**ISetGenerationService** → `SetGenerationService`
- Coordinates between hypertrophy and reps-per-set calculators
- Delegates to appropriate calculator based on progression type

**IHypertrophyCalculator** → `HypertrophyCalculator`
- Implements percentage-based set generation
- Uses HypertrophyOptions for configuration
- Handles plate rounding

**IRepsPerSetCalculator** → `RepsPerSetCalculator`
- Implements volume-based set generation
- Simple fixed-rep approach

---

## 11. Presentation Layer Contracts

### 11.1 API Endpoints

**Base URL:** `/api/workout-plans`

| Method | Endpoint | Purpose | Request | Response |
|--------|----------|---------|---------|----------|
| POST | `/` | Create workout plan | `CreateWorkoutPlanRequest` | `WorkoutPlanResponse` (201) |
| GET | `/{id}` | Get workout plan | - | `WorkoutPlanResponse` (200) |
| GET | `/{id}/current-week` | Get current week's sessions | - | `WeeklyPlanResponse` (200) |
| POST | `/{id}/complete-day` | Complete training day | `CompleteDayRequest` | `CompleteDayResponse` (200) |
| POST | `/{id}/apply-progression` | Apply weekly progression | `ApplyProgressionRequest` | `ProgressionResultResponse` (200) |

### 11.2 Request/Response Examples

**CreateWorkoutPlanRequest:**
```json
{
  "workoutName": "A2S Spring 2026",
  "workoutsPerWeek": 4,
  "exercises": [
    {
      "exerciseName": "Squat",
      "exerciseTemplateId": "squat_001",
      "restTimerSeconds": 180,
      "day": 1,
      "order": 0,
      "numberOfSets": 5,
      "bodyCategory": "Legs",
      "equipmentType": "Barbell",
      "progression": {
        "programType": "Hypertrophy",
        "trainingMax": 140.0,
        "weightProgression": 2.5,
        "attemptsBeforeDeload": 3,
        "isPrimary": true
      }
    },
    {
      "exerciseName": "Dumbbell Row",
      "exerciseTemplateId": "db_row_001",
      "restTimerSeconds": 90,
      "day": 1,
      "order": 1,
      "numberOfSets": 3,
      "bodyCategory": "Back",
      "equipmentType": "Dumbbell",
      "progression": {
        "programType": "RepsPerSet",
        "minimumReps": 8,
        "targetReps": 10,
        "maximumReps": 12,
        "startingSetCount": 3,
        "targetSetCount": 5,
        "startingWeight": 30.0
      }
    }
  ]
}
```

**WeeklyPlanResponse:**
```json
{
  "workoutPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "week": 1,
  "workoutsInWeek": 4,
  "days": {
    "1": [
      {
        "exerciseId": "abc123...",
        "exerciseName": "Squat",
        "restTimerSeconds": 180,
        "sets": [
          { "weightKg": 97.5, "reps": 5 },
          { "weightKg": 97.5, "reps": 5 },
          { "weightKg": 97.5, "reps": 5 },
          { "weightKg": 97.5, "reps": 5 },
          { "weightKg": 97.5, "reps": 10 }
        ]
      },
      {
        "exerciseId": "def456...",
        "exerciseName": "Dumbbell Row",
        "restTimerSeconds": 90,
        "sets": [
          { "weightKg": 30.0, "reps": 8 },
          { "weightKg": 30.0, "reps": 8 },
          { "weightKg": 30.0, "reps": 8 }
        ]
      }
    ],
    "2": [...],
    "3": [...],
    "4": [...]
  }
}
```

**CompleteDayRequest:**
```json
{
  "workoutPlanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "exercisePerformances": [
    {
      "exerciseId": "abc123...",
      "completedSets": [
        { "weightKg": 97.5, "reps": 5 },
        { "weightKg": 97.5, "reps": 5 },
        { "weightKg": 97.5, "reps": 5 },
        { "weightKg": 97.5, "reps": 5 },
        { "weightKg": 97.5, "reps": 12 }
      ],
      "performanceResult": "Success"
    },
    {
      "exerciseId": "def456...",
      "completedSets": [
        { "weightKg": 30.0, "reps": 8 },
        { "weightKg": 30.0, "reps": 8 },
        { "weightKg": 30.0, "reps": 8 }
      ],
      "performanceResult": "Success"
    }
  ]
}
```

---

## 12. Architectural Concerns & Improvements

Based on analysis of the current HevySync implementation, the following concerns and improvements should be addressed in the new application:

### 12.1 CRITICAL: Reps Per Set - Set Count Reset Issue

**Current Problem:**
In `RepsPerSetStrategy.ApplyProgression()` (lines 79-89), when a user reaches `TargetSetCount` and the system increases weight, it does NOT reset `StartingSetCount` back to the initial value.

**Current Code:**
```csharp
if (StartingSetCount < TargetSetCount)
{
    StartingSetCount++;
}
else
{
    // Reached target, increase weight
    StartingWeight += 2.5m;
    // BUG: StartingSetCount NOT reset!
}
```

**Expected Behavior:**
```csharp
if (StartingSetCount < TargetSetCount)
{
    StartingSetCount++;
}
else
{
    // Reached target, increase weight and reset sets
    StartingWeight += 2.5m;
    StartingSetCount = /* initial value - but what is it? */;
}
```

**Problem Analysis:**
- There's no property storing the "initial" set count
- Only `StartingSetCount` (current) and `TargetSetCount` (max) exist
- System cannot know what to reset to

**Proposed Solution:**
Add a new property `InitialSetCount` to `RepsPerSetProgression`:

```csharp
public sealed class RepsPerSetProgression : ExerciseProgression
{
    public RepRange RepRange { get; private set; }
    public int InitialSetCount { get; private set; }      // NEW: Never changes
    public int CurrentSetCount { get; private set; }      // RENAMED from StartingSetCount
    public int TargetSetCount { get; private set; }
    public decimal CurrentWeight { get; private set; }    // RENAMED from StartingWeight

    public void ApplyProgression(PerformanceResult result)
    {
        switch (result)
        {
            case PerformanceResult.Success:
                if (CurrentSetCount < TargetSetCount)
                {
                    CurrentSetCount++;
                }
                else
                {
                    CurrentWeight += 2.5m;
                    CurrentSetCount = InitialSetCount;  // FIXED: Reset to initial
                }
                break;

            case PerformanceResult.Failed:
                if (CurrentSetCount > InitialSetCount)  // CHANGED: Compare to initial, not 1
                {
                    CurrentSetCount--;
                }
                else if (CurrentWeight >= 2.5m)
                {
                    CurrentWeight -= 2.5m;
                }
                break;
        }
    }
}
```

**Impact:**
- Fixes the progression cycle to work as designed
- Prevents set count from continuously increasing
- Enables proper "reset and increase weight" behavior

---

### 12.2 Bounded Context - Exercise vs WorkoutPlan

**Current State:**
- Single bounded context "Workout Training Program"
- `Exercise` is an entity within the `WorkoutPlan` aggregate

**Concern:**
- Exercises could potentially be a separate aggregate with their own lifecycle
- However, in the A2S context, exercises have no meaning outside a workout plan
- Exercises are always created/deleted with the plan
- Exercises are modified only through the plan's progression logic

**Recommendation:**
- **Keep current design** - Exercise as entity within WorkoutPlan aggregate
- This enforces transactional consistency
- Simplified repository pattern (only WorkoutPlanRepository)
- Exercises cannot exist without a plan (domain rule)

**Alternative (NOT recommended for this domain):**
- Separate `Exercise` aggregate
- `WorkoutPlan` references exercises by ID
- Requires two repositories, eventual consistency, more complexity
- No clear benefit for this use case

---

### 12.3 Value Object - ExercisePerformance vs Entity

**Current State:**
`ExercisePerformance` is a value object used for passing performance data.

**Concern:**
- Performance history is not persisted
- Cannot track historical progress over time
- No audit trail of performance

**Recommendation for New System:**
- **Option A (Current approach - Simple):** Keep as value object, used only for commands
  - Pros: Simple, minimal persistence
  - Cons: No history

- **Option B (Enhanced - Recommended):** Create `PerformanceHistory` aggregate
  - New aggregate root: `WorkoutPerformanceHistory`
  - Entity: `DayPerformance` with `ExercisePerformance` list
  - Separate bounded context: "Performance Tracking"
  - Repository: `IPerformanceHistoryRepository`
  - Pros: Full audit trail, analytics capability, historical reports
  - Cons: Additional complexity

**If choosing Option B:**
```csharp
public sealed class WorkoutPerformanceHistory : AggregateRoot<Guid>
{
    private readonly List<DayPerformance> _performances = new();

    public Guid WorkoutPlanId { get; private set; }
    public Guid UserId { get; private set; }

    public IReadOnlyCollection<DayPerformance> Performances => _performances.AsReadOnly();

    public void RecordDayCompletion(
        int week,
        int day,
        List<ExercisePerformance> exercises)
    {
        var dayPerformance = DayPerformance.Create(week, day, exercises, DateTimeOffset.UtcNow);
        _performances.Add(dayPerformance);
    }
}

public sealed class DayPerformance : Entity<Guid>
{
    public int Week { get; private set; }
    public int Day { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }
    public List<ExercisePerformance> ExercisePerformances { get; private set; }
}
```

---

### 12.4 Domain Event Handling

**Current State:**
- Domain events are raised (WorkoutCreatedEvent, WorkoutDayCompletedEvent)
- No visible event handlers in the codebase

**Recommendation:**
- Add event handlers in Application layer for:
  - **Logging:** Log all significant domain events
  - **Analytics:** Track user engagement metrics (completion rates, progression trends)
  - **Notifications:** (Future) Notify user of milestones
  - **Performance History:** (If Option B above) Persist performance to history aggregate

**Example Handler:**
```csharp
public sealed class WorkoutDayCompletedEventHandler
    : IDomainEventHandler<WorkoutDayCompletedEvent>
{
    private readonly ILogger<WorkoutDayCompletedEventHandler> _logger;
    private readonly IPerformanceHistoryRepository _historyRepo; // If Option B

    public async Task HandleAsync(
        WorkoutDayCompletedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Workout day completed: {WorkoutId}, Week {Week}, Day {Day}",
            domainEvent.WorkoutPlanId,
            domainEvent.Week,
            domainEvent.Day);

        // Optionally persist to history
        var history = await _historyRepo.GetByWorkoutPlanIdAsync(
            domainEvent.WorkoutPlanId);

        history.RecordDayCompletion(
            domainEvent.Week,
            domainEvent.Day,
            domainEvent.Performances);

        await _historyRepo.SaveChangesAsync(cancellationToken);
    }
}
```

---

### 12.5 Validation - Domain vs Application

**Current State:**
- Value objects validate themselves (good)
- Aggregates validate invariants (good)
- Request validation seems minimal

**Recommendation:**
- **Domain Layer:** Validate business invariants (current approach is correct)
- **Application Layer:** Validate request structure, data types, ranges
- **Presentation Layer:** Validate HTTP-specific concerns (content-type, etc.)

**Add FluentValidation validators for all commands:**
```csharp
public class CreateWorkoutPlanCommandValidator
    : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.WorkoutName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.WorkoutsPerWeek)
            .InclusiveBetween(1, 7)
            .WithMessage("Workouts per week must be between 1 and 7");

        RuleFor(x => x.Exercises)
            .NotEmpty()
            .Must(exercises => exercises.All(e => e.Day <= x.WorkoutsPerWeek))
            .WithMessage("Exercise day cannot exceed workouts per week");

        RuleForEach(x => x.Exercises)
            .SetValidator(new CreateExerciseDtoValidator());
    }
}
```

---

### 12.6 Testing Gaps

**Current State:**
- Integration tests exist for happy paths
- Some validation tests
- Minimal unit test coverage for domain logic

**Recommendation:**
Add comprehensive unit tests for:

**Domain Layer:**
- All aggregate methods (Create, CompleteDay, ApplyProgression)
- All progression strategies (Success, Maintained, Failed paths)
- All value object validations
- Edge cases (minimum/maximum values)

**Application Layer:**
- All command handlers (with mocked repositories)
- All query handlers
- All validators

**Infrastructure Layer:**
- EF Core configurations (value object conversions)
- Repository implementations (with TestContainers)

**Presentation Layer:**
- All endpoints (with WebApplicationFactory)
- Error handling middleware
- Request/response serialization

---

### 12.7 Hypertrophy Configuration - Week Overflow

**Current Issue:**
Configuration arrays have 6 elements (weeks 1-6), but nothing prevents week number from exceeding 6.

**Current Code:**
```csharp
var weekIndex = activity.Week - 1; // What if Week = 7? Index = 6, out of bounds!
var intensity = config.Intensity[weekIndex]; // IndexOutOfRangeException
```

**Recommendation:**
Use modulo to cycle through the 6-week block:
```csharp
var weekIndex = (activity.Week - 1) % 6; // Cycles: 0,1,2,3,4,5,0,1,2...
var intensity = config.Intensity[weekIndex];
```

Or validate at configuration load:
```csharp
public class HypertrophyOptionsValidator : IValidateOptions<HypertrophyOptions>
{
    public ValidateOptionsResult Validate(string name, HypertrophyOptions options)
    {
        if (options.HypertrophyBlock.Primary.Intensity.Length != 6)
            return ValidateOptionsResult.Fail("Primary intensity must have exactly 6 values");

        if (options.HypertrophyBlock.Primary.RepsPerSet.Length != 6)
            return ValidateOptionsResult.Fail("Primary reps per set must have exactly 6 values");

        // ... validate all arrays

        return ValidateOptionsResult.Success;
    }
}
```

---

### 12.8 Concurrency

**Current State:**
No optimistic concurrency control

**Concern:**
If two users complete a day simultaneously, last write wins (data loss).

**Recommendation:**
Add `RowVersion` or `ConcurrencyToken` to `WorkoutPlan`:

```csharp
public sealed class WorkoutPlan : AggregateRoot<Guid>
{
    public byte[] RowVersion { get; private set; } = null!;
    // ... other properties
}

// In WorkoutPlanConfiguration
builder.Property(w => w.RowVersion)
    .IsRowVersion();

// In repository
public async Task UpdateAsync(WorkoutPlan workoutPlan, CancellationToken ct)
{
    try
    {
        _context.WorkoutPlans.Update(workoutPlan);
        await _context.SaveChangesAsync(ct);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        throw new ConcurrencyException(
            "Workout plan was modified by another process", ex);
    }
}
```

---

### 12.9 Nullable Reference Types

**Recommendation:**
Enable nullable reference types globally in all projects:

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

This catches null reference bugs at compile time.

---

### 12.10 Architecture Tests

**Recommendation:**
Add ArchUnitNET tests to enforce architectural rules:

```csharp
[Fact]
public void Domain_Should_Not_Reference_Other_Layers()
{
    var domain = Types.InAssembly(typeof(WorkoutPlan).Assembly);

    domain.Should()
        .NotDependOnAny("A2SWorkoutTracker.Application")
        .AndShould().NotDependOnAny("A2SWorkoutTracker.Infrastructure")
        .AndShould().NotDependOnAny("A2SWorkoutTracker.Api");
}

[Fact]
public void ValueObjects_Should_Be_Immutable()
{
    var valueObjects = Types.InAssembly(typeof(TrainingMax).Assembly)
        .That().ResideInNamespace("A2SWorkoutTracker.Domain.ValueObjects");

    valueObjects.Should()
        .BeRecords(); // All value objects should be records
}

[Fact]
public void Aggregates_Should_Have_Private_Constructors()
{
    var aggregates = Types.InAssembly(typeof(WorkoutPlan).Assembly)
        .That().Inherit(typeof(AggregateRoot<>));

    aggregates.Should()
        .HaveOnlyPrivateConstructors()
        .OrShould().HaveParameterlessConstructors(); // Except EF Core constructor
}
```

---

## 13. Pre-Implementation Requirements

Before writing any application code, the following scaffolding MUST be in place:

### 13.1 Coding Standards Documentation

Create the following files with coding standards (as defined in Section 4):

1. `<project-root>/AGENTS.md` - Root-level architecture guidelines
2. `<project-root>/docs/domain/coding-standards.md` - Domain layer standards
3. `<project-root>/docs/application/coding-standards.md` - Application layer standards
4. `<project-root>/docs/infrastructure/coding-standards.md` - Infrastructure layer standards
5. `<project-root>/docs/presentation/coding-standards.md` - Presentation layer standards

**These files are MANDATORY and must exist before any code is written.**

### 13.2 Solution Structure

Create the following projects:

```
A2SWorkoutTracker.sln
├── src/
│   ├── A2SWorkoutTracker.Domain/              (Class library, .NET 8+)
│   ├── A2SWorkoutTracker.Application/         (Class library, .NET 8+)
│   ├── A2SWorkoutTracker.Infrastructure/      (Class library, .NET 8+)
│   └── A2SWorkoutTracker.Api/                 (ASP.NET Core Web API, .NET 8+)
├── tests/
│   ├── A2SWorkoutTracker.Domain.UnitTests/    (xUnit test project)
│   ├── A2SWorkoutTracker.Application.UnitTests/ (xUnit test project)
│   └── A2SWorkoutTracker.IntegrationTests/    (xUnit test project)
├── docs/
│   ├── domain/
│   │   └── coding-standards.md
│   ├── application/
│   │   └── coding-standards.md
│   ├── infrastructure/
│   │   └── coding-standards.md
│   └── presentation/
│       └── coding-standards.md
└── AGENTS.md
```

### 13.3 NuGet Packages

**Domain:**
- None (zero dependencies except System.*)

**Application:**
- FluentValidation
- MediatR (optional, for command/query handlers)

**Infrastructure:**
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Design
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.Extensions.Options.DataAnnotations

**Presentation:**
- All Infrastructure packages (for DI registration)
- Swashbuckle.AspNetCore (Swagger/OpenAPI)

**Tests:**
- xUnit
- FluentAssertions
- FakeItEasy
- Testcontainers (for integration tests)
- Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory)

### 13.4 Project References

```
Domain          → (no references)
Application     → Domain
Infrastructure  → Domain, Application
Api             → Application, Infrastructure

Domain.UnitTests        → Domain
Application.UnitTests   → Application, Domain
IntegrationTests        → Api, Infrastructure, Application, Domain
```

### 13.5 Initial Checklist

Before writing domain code:

- [ ] All coding-standards.md files created and reviewed
- [ ] Solution structure matches specification
- [ ] All NuGet packages installed
- [ ] Project references configured correctly
- [ ] Nullable reference types enabled globally (`<Nullable>enable</Nullable>`)
- [ ] EditorConfig file created with formatting rules
- [ ] Git repository initialized with appropriate .gitignore
- [ ] CI/CD pipeline configured (GitHub Actions, Azure DevOps, etc.)
- [ ] Database connection string configured (appsettings.json)
- [ ] Logging configured
- [ ] Swagger configured

### 13.6 Development Order

Recommended implementation order:

**Phase 1: Domain Foundation**
1. Common base classes (Entity, AggregateRoot, ValueObject, DomainException)
2. Enums (PerformanceResult, BodyCategory, EquipmentType, ProgramType)
3. Value objects (all from Section 5.4)
4. Exercise progression entities (base class + both strategies)
5. Exercise entity
6. WorkoutPlan aggregate root
7. Domain events
8. Repository interfaces
9. Domain service interfaces
10. **Unit tests for all of the above**

**Phase 2: Application Layer**
1. Common interfaces (ICommand, ICommandHandler, IQuery, IQueryHandler)
2. DTOs (all request/response models)
3. CreateWorkoutPlanCommand + Handler + Validator
4. GetWorkoutPlanQuery + Handler
5. CompleteWorkoutDayCommand + Handler + Validator
6. ApplyProgressionCommand + Handler + Validator
7. GetCurrentWeekQuery + Handler
8. Domain event handlers
9. **Unit tests for all handlers and validators**

**Phase 3: Infrastructure Layer**
1. ApplicationDbContext
2. Entity configurations (all from Section 10.1)
3. Migrations
4. WorkoutPlanRepository implementation
5. HypertrophyCalculator implementation
6. RepsPerSetCalculator implementation
7. SetGenerationService implementation
8. Options classes (HypertrophyOptions)
9. **Integration tests for repositories and calculators**

**Phase 4: Presentation Layer**
1. Program.cs configuration
2. Endpoint classes
3. Request/Response DTOs (API-specific)
4. Exception handling middleware
5. Swagger configuration
6. **Integration tests for all endpoints**

**Phase 5: End-to-End Testing**
1. Full workflow tests (create → generate week → complete day → apply progression)
2. Cross-module tests (mixed Hypertrophy + RepsPerSet)
3. Performance tests (load testing)

---

## 14. Summary

This architectural specification provides a complete blueprint for building the A2S Workout Tracker application using Domain-Driven Design principles. The specification includes:

- **Comprehensive coding standards** for each layer of the onion architecture
- **Complete domain model** with aggregates, entities, value objects, and business rules
- **Detailed business logic** for both Hypertrophy and Reps Per Set modules
- **User stories** covering all happy path scenarios
- **Architectural patterns** for CQRS, repository, and domain services
- **Infrastructure requirements** including database schema and configuration
- **API contracts** with example requests and responses
- **Critical architectural concerns** identified from current implementation
- **Pre-implementation checklist** ensuring proper scaffolding

### Key Takeaways

1. **Domain Purity:** The Domain layer must have zero external dependencies
2. **Aggregate Boundaries:** WorkoutPlan is the aggregate root containing exercises
3. **Value Objects:** Use extensively for type safety and validation
4. **Progression Strategies:** Two distinct implementations (Hypertrophy vs RepsPerSet)
5. **Set Count Reset:** MUST be fixed in new implementation (see Section 12.1)
6. **Configuration-Driven:** Hypertrophy intensity patterns from configuration
7. **Test Coverage:** All layers require comprehensive automated tests

### Success Criteria

The new application will be considered complete when:

- All user stories (Section 8) are implemented and tested
- All coding standards (Section 4) are followed consistently
- All architectural concerns (Section 12) are addressed
- Test coverage exceeds 80% for Domain and Application layers
- Integration tests cover all API endpoints
- Database migrations are repeatable and versioned
- The application successfully runs end-to-end workflows
- Performance is acceptable (sub-second response times for all endpoints)

---

**End of Architectural Specification**

This document should serve as the single source of truth for all development decisions. Any deviations from this specification must be documented and justified.
