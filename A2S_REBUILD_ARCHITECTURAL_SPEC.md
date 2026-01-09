# Average 2 Savage (A2S) Workout Tracking Application - Architectural Specification

## Executive Summary

This document serves as the complete architectural specification for building a Domain-Driven Design (DDD) application to track the **Average 2 Savage** strength and hypertrophy training program. This application must enable users to create workout plans, track exercise performance across training blocks, and automatically progress exercises based on performance outcomes.

**Target Audience:** Software architects and domain experts implementing a greenfield DDD application.

**Scope:** End-to-end workout plan creation, exercise tracking, and progression for two distinct A2S training methodologies:
1. **Hypertrophy Module** (Linear Progression with percentage-based training)
2. **Reps Per Set Module** (Volume-based progression)

---

## Table of Contents

1. [Domain Overview](#1-domain-overview)
2. [Bounded Contexts](#2-bounded-contexts)
3. [Ubiquitous Language](#3-ubiquitous-language)
4. [Coding Standards by Layer](#4-coding-standards-by-layer)
5. [Domain Model](#5-domain-model)
6. [Business Logic - Hypertrophy Module](#6-business-logic---hypertrophy-module)
7. [Business Logic - Reps Per Set Module](#7-business-logic---reps-per-set-module)
8. [User Stories](#8-user-stories)
9. [Application Layer Patterns](#9-application-layer-patterns)
10. [Infrastructure Requirements](#10-infrastructure-requirements)
11. [Presentation Layer Contracts](#11-presentation-layer-contracts)
12. [Architectural Concerns & Improvements](#12-architectural-concerns--improvements)
13. [Pre-Implementation Requirements](#13-pre-implementation-requirements)

---

## 1. Domain Overview

### 1.1 Problem Domain

The **Average 2 Savage** program is a structured strength and hypertrophy training system where:
- Users create workout programs with multiple exercises
- Each exercise follows either a percentage-based or volume-based progression strategy
- Training is organized into weekly blocks with multiple training days per week
- Exercise difficulty automatically adjusts based on user performance (Success, Maintained, Failed)
- The system must track progress across 6-week training blocks with varying intensity patterns

### 1.2 Core Domain Concepts

**Workout** - A complete training program spanning multiple weeks, containing exercises distributed across training days.

**Exercise** - An individual movement (e.g., "Bench Press") with specific progression parameters.

**Progression Strategy** - The algorithm determining how an exercise advances in difficulty (weight, volume, or intensity).

**Training Block** - A structured 6-week period with pre-defined intensity and volume patterns (Hypertrophy module only).

**Performance Result** - User's outcome for a training session: Success (met targets), Maintained (no change needed), or Failed (missed targets/unable to complete).

**Workout Activity** - Tracks the current position within a training program (Week number, Day number).

---

## 2. Bounded Contexts

### 2.1 Primary Bounded Context: Workout Training Program

**Responsibility:** Manage the complete lifecycle of workout programs from creation through progression.

**Subdomains:**
- **Workout Management** - Create, retrieve, and organize workout programs
- **Exercise Progression** - Apply progression rules and calculate training parameters
- **Performance Tracking** - Record and evaluate exercise performance
- **Set Generation** - Calculate specific sets, reps, and weights for each training session

**Boundaries:**
- IN SCOPE: Workout creation, exercise tracking, progression calculation, performance history
- OUT OF SCOPE: External integrations (Hevy, Google Sheets), user authentication, nutritional tracking

### 2.2 Supporting Bounded Contexts (Future)

**Not part of initial implementation:**
- User Management (authentication, profiles)
- Integration Services (Hevy sync, third-party APIs)
- Analytics and Reporting

---

## 3. Ubiquitous Language

| Term | Definition | Usage Context |
|------|------------|---------------|
| **Workout** | A complete training program with exercises | "Create a new workout for the user" |
| **Exercise** | Individual movement within a workout | "Add Bench Press exercise to Day 1" |
| **Set** | Single instance of an exercise (weight × reps) | "Complete 5 sets of 100kg × 5 reps" |
| **Training Max (TM)** | Estimated one-rep maximum for an exercise | "Training max is 100kg for Squat" |
| **AMRAP** | As Many Reps As Possible - final set with no upper limit | "Last set is AMRAP with 70kg" |
| **Primary Lift** | Main compound movement (Squat, Bench, Deadlift) | "Squat is a primary lift with 5 working sets" |
| **Auxiliary Lift** | Secondary/accessory exercise | "Rows are auxiliary with 3 working sets" |
| **Intensity** | Percentage of training max used for a set | "Week 1 intensity is 70%" |
| **Volume** | Total work performed (sets × reps × weight) | "Increase volume by adding one set" |
| **Deload** | Reduction in training stress to facilitate recovery | "Failed week triggers a deload" |
| **Rep Range** | Minimum, target, and maximum rep boundaries | "Rep range is 8-10-12" |
| **Performance Result** | Outcome classification (Success/Maintained/Failed) | "User marked the session as Success" |
| **Workout Activity** | Current training position (Week, Day) | "Activity is Week 2, Day 3" |
| **Progression** | Advancement in exercise difficulty | "Apply progression after week completion" |
| **Training Block** | 6-week structured period with intensity variation | "Currently in Hypertrophy Block week 3" |

---

## 4. Coding Standards by Layer

### 4.1 ROOT - Architecture Guidelines (AGENTS.md)

**File Location:** `<project-root>/AGENTS.md`

```markdown
# A2S Workout Tracker - Architecture and Coding Standards

## Architecture and Code Organization (Clean Architecture / Onion Architecture)

- **Four layers:** Domain, Application, Infrastructure, Presentation
- **Dependency rule:** Outer layers depend on inner layers, never the reverse
  - Presentation → Application → Domain
  - Infrastructure → Domain (repositories, services)
  - Domain has ZERO external dependencies
- **Abstractions:** Expose interfaces between layers; implementations are internal and wired via DI
- **DTOs:** API endpoints MUST NOT expose domain models directly. Use DTOs in Presentation and Application layers

## Testing

- Run tests: `dotnet test`
- Frameworks: xUnit, FluentAssertions, FakeItEasy (for mocking if absolutely necessary)
- Test projects:
  - **Unit tests:** `*.Domain.UnitTests`, `*.Application.UnitTests` - Focus on business logic
  - **Integration tests:** `*.IntegrationTests` - Use WebApplicationFactory + Testcontainers for database
- **Coverage requirements:**
  - All domain entities, aggregates, and value objects MUST have unit tests
  - All command/query handlers MUST have unit tests
  - Happy path integration tests for all API endpoints
  - All architectural rules enforced via ArchUnitNET tests
- **Test-first development:** Write unit tests before or alongside implementation
- All changes MUST update existing tests if behavior changes
- New classes and methods MUST be covered by new unit tests

## Code Quality Principles

- **Expressiveness:** Code should be self-documenting. Use meaningful names over comments
- **Modern C#:** Use latest C# features (records, pattern matching, null-coalescing, etc.)
- **Immutability:** Prefer immutable types (records, readonly properties, init-only setters)
- **Explicit over implicit:** Avoid `var` when type isn't obvious. Be explicit about intent
- **Fail fast:** Validate at boundaries. Throw domain exceptions for invariant violations
- **No primitive obsession:** Wrap primitives in value objects when they have domain meaning
- **SOLID principles:** Single responsibility, Open/Closed, Liskov substitution, Interface segregation, Dependency inversion

## Git and Version Control

- **Branch naming:** `feature/<short-description>`, `bugfix/<issue-number>`
- **Commit messages:** Imperative mood ("Add training max validation" not "Added training max validation")
- **Pull requests:** All changes via PR with at least one review required
- **CI/CD:** All tests must pass before merging

## Solution Structure

```
A2SWorkoutTracker/
├── src/
│   ├── A2SWorkoutTracker.Domain/           # Core business logic
│   ├── A2SWorkoutTracker.Application/      # Use cases and orchestration
│   ├── A2SWorkoutTracker.Infrastructure/   # Data access, external services
│   └── A2SWorkoutTracker.Presentation/     # API endpoints, controllers
├── tests/
│   ├── A2SWorkoutTracker.Domain.UnitTests/
│   ├── A2SWorkoutTracker.Application.UnitTests/
│   └── A2SWorkoutTracker.IntegrationTests/
└── docs/
    ├── AGENTS.md                            # This file
    ├── domain/
    │   └── coding-standards.md
    ├── application/
    │   └── coding-standards.md
    ├── infrastructure/
    │   └── coding-standards.md
    └── presentation/
        └── coding-standards.md
```
```

---

### 4.2 DOMAIN LAYER - Coding Standards

**File Location:** `<project-root>/docs/domain/coding-standards.md`

```markdown
# Domain Layer - Coding Standards

## Scope and Precedence

- Applies to `A2SWorkoutTracker.Domain` project
- This is the innermost layer with ZERO external dependencies
- No references to Application, Infrastructure, or Presentation layers
- No persistence concerns, no external APIs, no frameworks (except basic System.* libraries)

## Core Responsibilities

- Define domain models: Aggregates, Entities, Value Objects
- Domain events for significant state changes
- Domain service interfaces (implementations live in Infrastructure)
- Domain exceptions for business rule violations
- Repository interfaces (implementations in Infrastructure)

## DDD Structure

```
A2SWorkoutTracker.Domain/
├── Aggregates/
│   └── WorkoutPlan/
│       ├── WorkoutPlan.cs                  # Aggregate root
│       ├── WorkoutPlanCreatedEvent.cs      # Domain event
│       ├── WorkoutPlanException.cs         # Domain exception
│       └── IWorkoutPlanRepository.cs       # Repository interface
├── Entities/
│   ├── Exercise.cs
│   ├── ExerciseProgression.cs              # Base class
│   ├── HypertrophyProgression.cs
│   ├── RepsPerSetProgression.cs
│   └── ExerciseException.cs
├── ValueObjects/
│   ├── WorkoutName.cs
│   ├── ExerciseName.cs
│   ├── TrainingMax.cs
│   ├── WeightProgression.cs
│   ├── RepRange.cs
│   ├── Set.cs
│   ├── ExercisePerformance.cs
│   ├── WorkoutActivity.cs
│   └── RestTimer.cs
├── DomainServices/
│   ├── ISetGenerationService.cs
│   ├── IHypertrophyCalculator.cs
│   └── IRepsPerSetCalculator.cs
├── Enums/
│   ├── PerformanceResult.cs
│   ├── BodyCategory.cs
│   └── EquipmentType.cs
└── Common/
    ├── Entity.cs
    ├── AggregateRoot.cs
    ├── ValueObject.cs
    └── DomainException.cs
```

## Aggregates

**Rules:**
1. **Aggregate roots** are the root of all change - only they can be directly loaded/saved
2. **Consistency boundary:** All business rules within an aggregate must be consistent
3. **Small aggregates:** Prefer small aggregates over large complex graphs
4. **Transactional boundary:** One aggregate per transaction
5. **Reference by ID:** Aggregates reference other aggregates by ID only, not object references
6. **Invariant protection:** All state changes go through public methods that enforce invariants

**Pattern:**
```csharp
public sealed class WorkoutPlan : AggregateRoot<Guid>
{
    private readonly List<Exercise> _exercises = new();

    public WorkoutName Name { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public WorkoutActivity Activity { get; private set; } = null!;

    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    // EF Core constructor (protected, parameterless)
    private WorkoutPlan() { }

    // Factory method for creation
    public static WorkoutPlan Create(
        WorkoutName name,
        Guid userId,
        int workoutsPerWeek,
        List<Exercise> exercises)
    {
        // Validate invariants
        if (userId == Guid.Empty)
            throw new WorkoutPlanException("User ID cannot be empty");

        if (exercises == null || exercises.Count == 0)
            throw new WorkoutPlanException("Workout must contain at least one exercise");

        var workout = new WorkoutPlan(
            Guid.NewGuid(),
            name,
            userId,
            WorkoutActivity.CreateInitial(workoutsPerWeek),
            exercises);

        // Raise domain event
        workout.RaiseDomainEvent(new WorkoutPlanCreatedEvent(
            workout.Id,
            workout.Name,
            workout.UserId,
            DateTimeOffset.UtcNow));

        return workout;
    }

    // Business method enforcing rules
    public void CompleteDay(List<ExercisePerformance> performances)
    {
        // Validate
        // Execute business logic
        // Raise domain events
        // Update state
    }
}
```

**Naming:**
- Aggregate root files: `<AggregateName>.cs` (e.g., `WorkoutPlan.cs`)
- Organize in folder: `Aggregates/<AggregateName>/`

## Entities

**Rules:**
1. **Identity:** All entities have a unique ID (Guid or int)
2. **Belong to aggregates:** Entities can only be accessed through their aggregate root
3. **No direct persistence:** Only aggregate roots have repositories
4. **Immutable IDs:** Entity IDs never change after creation
5. **Factory methods:** Use static `Create()` methods for construction with validation
6. **EF Core constructor:** Provide protected parameterless constructor for EF Core (mark with `[ExcludeFromCodeCoverage]`)

**Pattern:**
```csharp
public sealed class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; private set; } = null!;
    public int Day { get; private set; }
    public int Order { get; private set; }
    public Guid WorkoutPlanId { get; private set; }
    public ExerciseProgression Progression { get; private set; } = null!;

    // EF Core constructor
    [ExcludeFromCodeCoverage(Justification = "Needed for EF Core")]
    private Exercise() { }

    private Exercise(
        Guid id,
        ExerciseName name,
        int day,
        int order,
        Guid workoutPlanId,
        ExerciseProgression progression) : base(id)
    {
        Name = name;
        Day = day;
        Order = order;
        WorkoutPlanId = workoutPlanId;
        Progression = progression;
    }

    public static Exercise Create(
        ExerciseName name,
        int day,
        int order,
        Guid workoutPlanId,
        ExerciseProgression progression)
    {
        if (day < 1)
            throw new ArgumentException("Day must be at least 1", nameof(day));

        return new Exercise(
            Guid.NewGuid(),
            name,
            day,
            order,
            workoutPlanId,
            progression);
    }
}
```

## Value Objects

**Rules:**
1. **Immutable:** All properties init-only or readonly
2. **Structural equality:** Two value objects with same values are equal
3. **Self-validating:** Constructor validates all invariants
4. **No identity:** Value objects don't have IDs
5. **Records preferred:** Use C# `record` for concise syntax
6. **Factory methods:** Static `Create()` methods for construction

**Pattern:**
```csharp
public sealed record TrainingMax
{
    public decimal Value { get; init; }

    private TrainingMax(decimal value)
    {
        Value = value;
    }

    public static TrainingMax Create(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("Training max must be greater than zero", nameof(value));

        return new TrainingMax(value);
    }

    public static implicit operator decimal(TrainingMax trainingMax) => trainingMax.Value;
}
```

**Complex Value Objects:**
```csharp
public sealed record RepRange
{
    public int MinimumReps { get; init; }
    public int TargetReps { get; init; }
    public int MaximumReps { get; init; }

    private RepRange(int minimumReps, int targetReps, int maximumReps)
    {
        MinimumReps = minimumReps;
        TargetReps = targetReps;
        MaximumReps = maximumReps;
    }

    public static RepRange Create(int minimumReps, int targetReps, int maximumReps)
    {
        if (minimumReps < 1)
            throw new ArgumentException("Minimum reps must be at least 1");
        if (targetReps < minimumReps)
            throw new ArgumentException("Target reps must be >= minimum reps");
        if (maximumReps < targetReps)
            throw new ArgumentException("Maximum reps must be >= target reps");

        return new RepRange(minimumReps, targetReps, maximumReps);
    }
}
```

## Domain Services

**Rules:**
1. **Stateless:** Domain services should not hold state
2. **Interfaces in Domain:** Define interfaces in Domain layer
3. **Implementations in Infrastructure:** Concrete classes live in Infrastructure
4. **Pure domain logic:** Only for operations that don't naturally fit in entities/value objects
5. **Named with domain language:** Use ubiquitous language (not technical terms)

**Pattern:**
```csharp
// Domain/DomainServices/IHypertrophyCalculator.cs
public interface IHypertrophyCalculator
{
    Task<List<Set>> CalculateSetsForWeekAsync(
        HypertrophyProgression progression,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default);
}
```

## Domain Events

**Rules:**
1. **Past tense:** Events represent things that have happened
2. **Immutable:** All properties init-only
3. **Records:** Use C# records for concise syntax
4. **Timestamp:** Include `DateTimeOffset OccurredOn` property
5. **Aggregate ID:** Include aggregate root ID
6. **Rich data:** Include relevant data for subscribers (avoid forcing queries)

**Pattern:**
```csharp
public sealed record WorkoutDayCompletedEvent(
    Guid WorkoutPlanId,
    int Week,
    int Day,
    List<ExercisePerformance> Performances,
    DateTimeOffset OccurredOn);
```

## Domain Exceptions

**Rules:**
1. **Inherit from DomainException:** Base class for all domain exceptions
2. **Meaningful names:** Exception name indicates the problem
3. **Business language:** Error messages use ubiquitous language
4. **Constructors:** Accept message and optionally inner exception

**Pattern:**
```csharp
public sealed class WorkoutPlanException : DomainException
{
    public WorkoutPlanException(string message) : base(message) { }

    public WorkoutPlanException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

## Testing

**Unit Test Requirements:**
- All aggregate roots MUST have comprehensive tests for:
  - Creation (factory methods)
  - Business methods (state transitions)
  - Invariant validation (exception cases)
  - Domain event raising
- All value objects MUST test:
  - Creation with valid values
  - Validation failures
  - Equality semantics
- Use testing utilities from `A2SWorkoutTracker.Domain.Testing` package
- Tests MUST NOT require a database or external dependencies

**Test Naming:**
- `<MethodName>_<Scenario>_<ExpectedResult>`
- Example: `Create_WithEmptyUserId_ThrowsWorkoutPlanException`

## Additional Rules

1. **No public setters:** All properties must be `{ get; private set; }` or `{ get; init; }`
2. **No anemic models:** Entities should have behavior, not just data
3. **Guard clauses:** Validate method parameters at the start of methods
4. **Async all the way:** Use async/await for any I/O operations in domain service interfaces
5. **CancellationToken:** Include `CancellationToken cancellationToken = default` for async operations
6. **Null safety:** Use nullable reference types (`#nullable enable`)
7. **No magic numbers:** Use constants or configuration
8. **Enums for fixed sets:** Use enums for fixed value sets (PerformanceResult, BodyCategory)
```

---

### 4.3 APPLICATION LAYER - Coding Standards

**File Location:** `<project-root>/docs/application/coding-standards.md`

```markdown
# Application Layer - Coding Standards

## Scope and Precedence

- Applies to `A2SWorkoutTracker.Application` project
- Orchestrates domain objects to fulfill use cases
- References: Domain layer only
- No references to Infrastructure or Presentation

## Core Responsibilities

- Encapsulate use cases via Commands and Queries (CQRS pattern)
- Coordinate domain objects and domain services
- Transaction management (Unit of Work pattern)
- DTO mapping (Domain ↔ Application DTOs)
- Application-level validation (request validation)
- Domain event handlers (in-process reactions)

## Project Structure

```
A2SWorkoutTracker.Application/
├── WorkoutPlans/
│   ├── Commands/
│   │   ├── CreateWorkoutPlan/
│   │   │   ├── CreateWorkoutPlanCommand.cs
│   │   │   ├── CreateWorkoutPlanCommandHandler.cs
│   │   │   └── CreateWorkoutPlanCommandValidator.cs
│   │   ├── CompleteWorkoutDay/
│   │   │   ├── CompleteWorkoutDayCommand.cs
│   │   │   ├── CompleteWorkoutDayCommandHandler.cs
│   │   │   └── CompleteWorkoutDayCommandValidator.cs
│   │   └── ApplyProgression/
│   │       ├── ApplyProgressionCommand.cs
│   │       └── ApplyProgressionCommandHandler.cs
│   ├── Queries/
│   │   ├── GetWorkoutPlan/
│   │   │   ├── GetWorkoutPlanQuery.cs
│   │   │   ├── GetWorkoutPlanQueryHandler.cs
│   │   │   └── WorkoutPlanDto.cs
│   │   └── GetCurrentWeek/
│   │       ├── GetCurrentWeekQuery.cs
│   │       ├── GetCurrentWeekQueryHandler.cs
│   │       └── WeeklyPlanDto.cs
│   └── DomainEventHandlers/
│       ├── WorkoutPlanCreatedEventHandler.cs
│       └── WorkoutDayCompletedEventHandler.cs
├── Common/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── IQuery.cs
│   ├── IQueryHandler.cs
│   └── Result.cs
└── DependencyInjection.cs
```

## Commands (Write Operations)

**Rules:**
1. **Command = Intent:** Commands represent user intent to change state
2. **Immutable:** Commands are immutable records
3. **One handler per command:** 1:1 relationship between command and handler
4. **Return type:** Return DTOs or Result<T> (not domain objects)
5. **Validation:** Use FluentValidation for command validation
6. **Transactions:** Handlers manage transactions (via Unit of Work or SaveChangesAsync)

**Pattern:**
```csharp
// CreateWorkoutPlanCommand.cs
public sealed record CreateWorkoutPlanCommand(
    string WorkoutName,
    Guid UserId,
    int WorkoutsPerWeek,
    List<CreateExerciseDto> Exercises) : ICommand<WorkoutPlanDto>;

// CreateWorkoutPlanCommandHandler.cs
public sealed class CreateWorkoutPlanCommandHandler
    : ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>
{
    private readonly IWorkoutPlanRepository _repository;

    public CreateWorkoutPlanCommandHandler(IWorkoutPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkoutPlanDto> HandleAsync(
        CreateWorkoutPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Map command DTOs to domain value objects
        var workoutName = WorkoutName.Create(command.WorkoutName);

        // 2. Create domain entities
        var exercises = command.Exercises.Select(dto => MapToExercise(dto)).ToList();

        // 3. Create aggregate root (business logic)
        var workoutPlan = WorkoutPlan.Create(
            workoutName,
            command.UserId,
            command.WorkoutsPerWeek,
            exercises);

        // 4. Persist
        await _repository.AddAsync(workoutPlan, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // 5. Map to DTO and return
        return MapToDto(workoutPlan);
    }

    private Exercise MapToExercise(CreateExerciseDto dto)
    {
        // Mapping logic
    }

    private WorkoutPlanDto MapToDto(WorkoutPlan workoutPlan)
    {
        // Mapping logic
    }
}

// CreateWorkoutPlanCommandValidator.cs
public sealed class CreateWorkoutPlanCommandValidator
    : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.WorkoutName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.WorkoutsPerWeek)
            .InclusiveBetween(1, 7);

        RuleFor(x => x.Exercises)
            .NotEmpty()
            .Must(x => x.Count > 0)
            .WithMessage("At least one exercise is required");
    }
}
```

## Queries (Read Operations)

**Rules:**
1. **Query = Question:** Queries represent questions about current state
2. **Immutable:** Queries are immutable records
3. **No side effects:** Queries MUST NOT modify state
4. **Return DTOs:** Return application-specific DTOs optimized for the UI
5. **Projection:** Use EF projections for performance (avoid loading full aggregates)

**Pattern:**
```csharp
// GetWorkoutPlanQuery.cs
public sealed record GetWorkoutPlanQuery(Guid WorkoutPlanId) : IQuery<WorkoutPlanDto>;

// GetWorkoutPlanQueryHandler.cs
public sealed class GetWorkoutPlanQueryHandler
    : IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDto>
{
    private readonly IWorkoutPlanRepository _repository;

    public GetWorkoutPlanQueryHandler(IWorkoutPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkoutPlanDto> HandleAsync(
        GetWorkoutPlanQuery query,
        CancellationToken cancellationToken = default)
    {
        var workoutPlan = await _repository.GetByIdWithExercisesAsync(
            query.WorkoutPlanId,
            cancellationToken);

        if (workoutPlan == null)
            throw new NotFoundException($"Workout plan {query.WorkoutPlanId} not found");

        return MapToDto(workoutPlan);
    }
}
```

## DTOs (Data Transfer Objects)

**Rules:**
1. **Simple POCOs:** No behavior, just data
2. **Serializable:** Must be JSON serializable
3. **Flat structures:** Avoid deep nesting (max 2-3 levels)
4. **Nullable annotations:** Use `?` for optional fields
5. **Records or classes:** Use records for immutability, classes if mutable needed
6. **Naming:** Suffix with `Dto` (e.g., `WorkoutPlanDto`, `ExerciseDto`)

**Pattern:**
```csharp
public sealed record WorkoutPlanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public WorkoutActivityDto Activity { get; init; } = null!;
    public List<ExerciseDto> Exercises { get; init; } = new();
}

public sealed record ExerciseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Day { get; init; }
    public int Order { get; init; }
    public ExerciseProgressionDto Progression { get; init; } = null!;
}

// Polymorphic DTOs using abstract base
public abstract record ExerciseProgressionDto
{
    public Guid Id { get; init; }
    public string ProgramType { get; init; } = string.Empty;
}

public sealed record HypertrophyProgressionDto : ExerciseProgressionDto
{
    public decimal TrainingMax { get; init; }
    public decimal WeightProgression { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record RepsPerSetProgressionDto : ExerciseProgressionDto
{
    public int MinimumReps { get; init; }
    public int TargetReps { get; init; }
    public int MaximumReps { get; init; }
    public int CurrentSetCount { get; init; }
    public int TargetSetCount { get; init; }
    public decimal CurrentWeight { get; init; }
}
```

## Domain Event Handlers

**Rules:**
1. **In-process only:** Application layer handles in-process domain events
2. **Side effects:** Use for application-level side effects (sending emails, logging, etc.)
3. **Async:** Event handlers should be async
4. **Transaction boundary:** Decide if handlers run in same transaction or separate
5. **Multiple handlers:** Multiple handlers can subscribe to same event

**Pattern:**
```csharp
public sealed class WorkoutDayCompletedEventHandler
    : IDomainEventHandler<WorkoutDayCompletedEvent>
{
    private readonly ILogger<WorkoutDayCompletedEventHandler> _logger;

    public WorkoutDayCompletedEventHandler(
        ILogger<WorkoutDayCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        WorkoutDayCompletedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Workout day completed: WorkoutId={WorkoutId}, Week={Week}, Day={Day}",
            domainEvent.WorkoutPlanId,
            domainEvent.Week,
            domainEvent.Day);

        // Additional side effects (send notification, update analytics, etc.)
        await Task.CompletedTask;
    }
}
```

## Dependency Injection

**Rules:**
1. **Extension methods:** Use `IServiceCollection` extension methods
2. **Scoped lifetime:** Handlers are typically scoped
3. **Auto-registration:** Use reflection to auto-register handlers
4. **Validators:** Register FluentValidation validators

**Pattern:**
```csharp
// DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Register all command handlers
        services.AddScoped<ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto>,
            CreateWorkoutPlanCommandHandler>();

        // Register all query handlers
        services.AddScoped<IQueryHandler<GetWorkoutPlanQuery, WorkoutPlanDto>,
            GetWorkoutPlanQueryHandler>();

        // Register FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateWorkoutPlanCommandValidator>();

        // Register domain event handlers
        services.AddScoped<IDomainEventHandler<WorkoutDayCompletedEvent>,
            WorkoutDayCompletedEventHandler>();

        return services;
    }
}
```

## Testing

**Unit Test Requirements:**
- All command handlers MUST have tests for:
  - Happy path (successful execution)
  - Validation failures (via validators)
  - Business logic exceptions
- All query handlers MUST have tests for:
  - Happy path (data retrieval)
  - Not found scenarios
- Mock repositories using FakeItEasy
- Test DTOs separately for serialization/deserialization

**Test Pattern:**
```csharp
public class CreateWorkoutPlanCommandHandlerTests
{
    private readonly IWorkoutPlanRepository _repository;
    private readonly CreateWorkoutPlanCommandHandler _handler;

    public CreateWorkoutPlanCommandHandlerTests()
    {
        _repository = A.Fake<IWorkoutPlanRepository>();
        _handler = new CreateWorkoutPlanCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesWorkoutPlan()
    {
        // Arrange
        var command = new CreateWorkoutPlanCommand(
            "Test Workout",
            Guid.NewGuid(),
            5,
            new List<CreateExerciseDto> { /* ... */ });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Workout");
        A.CallTo(() => _repository.AddAsync(
            A<WorkoutPlan>._,
            A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
```

## Additional Rules

1. **No direct database access:** Use repositories only
2. **No business logic:** Business logic belongs in Domain layer
3. **Thin handlers:** Handlers should orchestrate, not implement business rules
4. **Mapping:** Consider using AutoMapper or manual mapping (avoid magic)
5. **Error handling:** Let domain exceptions bubble up; catch and wrap infrastructure exceptions
6. **Logging:** Log at handler level (input, output, exceptions)
```

---

### 4.4 INFRASTRUCTURE LAYER - Coding Standards

**File Location:** `<project-root>/docs/infrastructure/coding-standards.md`

```markdown
# Infrastructure Layer - Coding Standards

## Scope and Precedence

- Applies to `A2SWorkoutTracker.Infrastructure` project
- Implements technical concerns and external dependencies
- References: Domain, Application layers
- Contains: Database access, external APIs, file system, third-party integrations

## Core Responsibilities

- Repository implementations (Entity Framework Core)
- Domain service implementations (calculations, complex algorithms)
- Database context and migrations
- Entity configurations (Fluent API)
- External service integrations
- Configuration options classes

## Project Structure

```
A2SWorkoutTracker.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   ├── Repositories/
│   │   └── WorkoutPlanRepository.cs
│   ├── Configurations/
│   │   ├── WorkoutPlanConfiguration.cs
│   │   ├── ExerciseConfiguration.cs
│   │   └── ExerciseProgressionConfiguration.cs
│   └── Migrations/
│       └── 20260109_Initial.cs
├── DomainServices/
│   ├── SetGenerationService.cs
│   ├── HypertrophyCalculator.cs
│   └── RepsPerSetCalculator.cs
├── Options/
│   ├── HypertrophyOptions.cs
│   └── DatabaseOptions.cs
└── DependencyInjection.cs
```

## Repository Implementations

**Rules:**
1. **Implement domain interfaces:** Repositories implement interfaces from Domain layer
2. **EF Core:** Use Entity Framework Core for data access
3. **Async all the way:** All database operations must be async
4. **Include related data:** Provide methods with/without eager loading
5. **No business logic:** Repositories only handle persistence, no business rules

**Pattern:**
```csharp
public sealed class WorkoutPlanRepository : IWorkoutPlanRepository
{
    private readonly ApplicationDbContext _context;

    public WorkoutPlanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkoutPlan?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutPlans
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkoutPlan?> GetByIdWithExercisesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutPlans
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Progression)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<List<WorkoutPlan>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutPlans
            .Where(w => w.UserId == userId)
            .Include(w => w.Exercises)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        WorkoutPlan workoutPlan,
        CancellationToken cancellationToken = default)
    {
        await _context.WorkoutPlans.AddAsync(workoutPlan, cancellationToken);
    }

    public Task UpdateAsync(
        WorkoutPlan workoutPlan,
        CancellationToken cancellationToken = default)
    {
        _context.WorkoutPlans.Update(workoutPlan);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

## DbContext

**Rules:**
1. **One DbContext:** Single context for the application
2. **DbSet properties:** Public DbSet<T> for all aggregate roots and entities
3. **Apply configurations:** Use Fluent API configurations via `OnModelCreating`
4. **No business logic:** DbContext is purely for data access
5. **Connection string:** Injected via options pattern

**Pattern:**
```csharp
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkoutPlan> WorkoutPlans => Set<WorkoutPlan>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseProgression> ExerciseProgressions => Set<ExerciseProgression>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

## Entity Configurations (Fluent API)

**Rules:**
1. **One configuration per entity:** Separate class for each entity type
2. **Implement IEntityTypeConfiguration<T>:** Use type-safe configuration interface
3. **Value object conversions:** Map value objects to database columns
4. **Owned types:** Use `OwnsOne` for complex value objects
5. **Table Per Hierarchy (TPH):** Use discriminators for inheritance
6. **Relationships:** Configure all relationships explicitly

**Pattern:**
```csharp
// WorkoutPlanConfiguration.cs
public sealed class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.ToTable("workout_plans");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Value object conversion
        builder.Property(w => w.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired()
            .HasConversion(
                name => name.Value,
                value => WorkoutName.Create(value));

        // Owned type (complex value object)
        builder.OwnsOne(w => w.Activity, activity =>
        {
            activity.Property(a => a.Week)
                .HasColumnName("current_week")
                .IsRequired();

            activity.Property(a => a.Day)
                .HasColumnName("current_day")
                .IsRequired();

            activity.Property(a => a.WorkoutsInWeek)
                .HasColumnName("workouts_in_week")
                .IsRequired();
        });

        // Relationship to exercises
        builder.HasMany(w => w.Exercises)
            .WithOne()
            .HasForeignKey("WorkoutPlanId")
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field for private collection
        builder.Metadata.FindNavigation(nameof(WorkoutPlan.Exercises))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

// ExerciseProgressionConfiguration.cs (TPH for inheritance)
public sealed class ExerciseProgressionConfiguration
    : IEntityTypeConfiguration<ExerciseProgression>
{
    public void Configure(EntityTypeBuilder<ExerciseProgression> builder)
    {
        builder.ToTable("exercise_progressions");

        builder.HasKey(p => p.Id);

        // Discriminator for TPH
        builder.HasDiscriminator<string>("progression_type")
            .HasValue<HypertrophyProgression>("Hypertrophy")
            .HasValue<RepsPerSetProgression>("RepsPerSet");

        builder.Property(p => p.ExerciseId)
            .HasColumnName("exercise_id")
            .IsRequired();
    }
}

public sealed class HypertrophyProgressionConfiguration
    : IEntityTypeConfiguration<HypertrophyProgression>
{
    public void Configure(EntityTypeBuilder<HypertrophyProgression> builder)
    {
        builder.Property(p => p.TrainingMax)
            .HasColumnName("training_max")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                tm => tm.Value,
                value => TrainingMax.Create(value));

        builder.Property(p => p.WeightProgression)
            .HasColumnName("weight_progression")
            .HasPrecision(18, 2)
            .IsRequired()
            .HasConversion(
                wp => wp.Value,
                value => WeightProgression.Create(value));

        builder.Property(p => p.IsPrimary)
            .HasColumnName("is_primary")
            .IsRequired();
    }
}
```

## Domain Service Implementations

**Rules:**
1. **Implement domain interfaces:** Services implement interfaces from Domain layer
2. **Stateless:** No instance state (use DI for dependencies)
3. **Configuration:** Use Options pattern for configuration
4. **Business logic allowed:** Can contain complex calculations/algorithms
5. **Pure functions preferred:** Avoid side effects when possible

**Pattern:**
```csharp
public sealed class HypertrophyCalculator : IHypertrophyCalculator
{
    private readonly HypertrophyOptions _options;
    private readonly ILogger<HypertrophyCalculator> _logger;

    public HypertrophyCalculator(
        IOptions<HypertrophyOptions> options,
        ILogger<HypertrophyCalculator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task<List<Set>> CalculateSetsForWeekAsync(
        HypertrophyProgression progression,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default)
    {
        var config = progression.IsPrimary
            ? _options.HypertrophyBlock.Primary
            : _options.HypertrophyBlock.Auxiliary;

        var weekIndex = activity.Week - 1; // 0-based

        var workingWeight = CalculateWorkingWeight(
            progression.TrainingMax.Value,
            config.Intensity[weekIndex]);

        var sets = new List<Set>();

        // Create working sets
        for (var i = 0; i < config.Sets; i++)
        {
            var reps = (i == config.Sets - 1)
                ? config.AmrapRepTarget[weekIndex]  // Last set is AMRAP
                : config.RepsPerSet[weekIndex];     // Regular sets

            sets.Add(Set.Create(workingWeight, reps));
        }

        _logger.LogDebug(
            "Generated {SetCount} sets for week {Week}: {Weight}kg",
            sets.Count,
            activity.Week,
            workingWeight);

        return Task.FromResult(sets);
    }

    private decimal CalculateWorkingWeight(decimal trainingMax, decimal intensity)
    {
        var rawWeight = trainingMax * intensity;
        return RoundToNearestPlate(rawWeight, _options.DefaultRoundingValue);
    }

    private static decimal RoundToNearestPlate(decimal weight, decimal plateIncrement)
    {
        return Math.Round(weight / plateIncrement) * plateIncrement;
    }
}
```

## Options Classes

**Rules:**
1. **POCO classes:** Simple classes with properties
2. **Section name constant:** Define `SectionName` constant for configuration binding
3. **Validation:** Use DataAnnotations or FluentValidation
4. **Nested options:** Use nested classes for complex configuration

**Pattern:**
```csharp
public sealed class HypertrophyOptions
{
    public const string SectionName = "Hypertrophy";

    [Range(0.01, 10.0)]
    public decimal DefaultRoundingValue { get; set; } = 1.25m;

    public HypertrophyBlockOptions HypertrophyBlock { get; set; } = new();
}

public sealed class HypertrophyBlockOptions
{
    public LiftTypeOptions Primary { get; set; } = new();
    public LiftTypeOptions Auxiliary { get; set; } = new();
}

public sealed class LiftTypeOptions
{
    [Required]
    [MinLength(6)]
    public int[] AmrapRepTarget { get; set; } = Array.Empty<int>();

    [Required]
    [MinLength(6)]
    public int[] RepsPerSet { get; set; } = Array.Empty<int>();

    [Required]
    [MinLength(6)]
    public decimal[] Intensity { get; set; } = Array.Empty<decimal>();

    [Range(1, 10)]
    public int Sets { get; set; }
}
```

## Dependency Injection

**Pattern:**
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();

        // Domain services
        services.AddScoped<ISetGenerationService, SetGenerationService>();
        services.AddScoped<IHypertrophyCalculator, HypertrophyCalculator>();
        services.AddScoped<IRepsPerSetCalculator, RepsPerSetCalculator>();

        // Options
        services.Configure<HypertrophyOptions>(
            configuration.GetSection(HypertrophyOptions.SectionName));

        services.AddOptionsWithValidateOnStart<HypertrophyOptions>();

        return services;
    }
}
```

## Testing

**Integration Test Requirements:**
- Use Testcontainers for database (PostgreSQL container)
- Test repositories with real database
- Test EF configurations (value object conversions, relationships)
- Test migrations (can apply, rollback)

**Test Pattern:**
```csharp
public class WorkoutPlanRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly ApplicationDbContext _context;
    private readonly WorkoutPlanRepository _repository;

    public WorkoutPlanRepositoryTests(DatabaseFixture fixture)
    {
        _context = fixture.CreateContext();
        _repository = new WorkoutPlanRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ValidWorkoutPlan_PersistsToDatabase()
    {
        // Arrange
        var workoutPlan = WorkoutPlan.Create(
            WorkoutName.Create("Test"),
            Guid.NewGuid(),
            5,
            new List<Exercise>());

        // Act
        await _repository.AddAsync(workoutPlan);
        await _repository.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(workoutPlan.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Value.Should().Be("Test");
    }
}
```
```

---

### 4.5 PRESENTATION LAYER - Coding Standards

**File Location:** `<project-root>/docs/presentation/coding-standards.md`

```markdown
# Presentation Layer - Coding Standards

## Scope and Precedence

- Applies to `A2SWorkoutTracker.Presentation` project (or `A2SWorkoutTracker.Api`)
- HTTP API endpoints, request/response handling
- References: Application, Infrastructure layers (NOT Domain directly)
- ASP.NET Core Minimal APIs or Controllers

## Core Responsibilities

- HTTP endpoints (REST API)
- Request/Response DTOs (different from Application DTOs)
- Request validation
- Authentication/Authorization
- HTTP status code mapping
- API documentation (OpenAPI/Swagger)
- Middleware configuration

## Project Structure

```
A2SWorkoutTracker.Api/
├── Program.cs
├── appsettings.json
├── Endpoints/
│   └── WorkoutPlans/
│       ├── WorkoutPlanEndpoints.cs
│       ├── Requests/
│       │   ├── CreateWorkoutPlanRequest.cs
│       │   └── CompleteWorkoutDayRequest.cs
│       └── Responses/
│           ├── WorkoutPlanResponse.cs
│           └── ErrorResponse.cs
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs
│   └── ValidationMiddleware.cs
└── Extensions/
    └── ResultExtensions.cs
```

## Minimal API Endpoints

**Rules:**
1. **Minimal APIs preferred:** Use ASP.NET Core Minimal APIs over Controllers
2. **Grouped by feature:** Organize endpoints by aggregate/feature
3. **Separate request/response:** Don't expose Application DTOs directly
4. **HTTP verbs:** Use correct HTTP verbs (POST for commands, GET for queries)
5. **Status codes:** Return appropriate HTTP status codes

**Pattern:**
```csharp
public static class WorkoutPlanEndpoints
{
    public static void MapWorkoutPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workout-plans")
            .WithTags("Workout Plans")
            .RequireAuthorization();

        group.MapPost("/", CreateWorkoutPlan)
            .WithName("CreateWorkoutPlan")
            .Produces<WorkoutPlanResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPost("/{workoutPlanId}/complete-day", CompleteWorkoutDay)
            .WithName("CompleteWorkoutDay")
            .Produces<CompleteDayResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{workoutPlanId}", GetWorkoutPlan)
            .WithName("GetWorkoutPlan")
            .Produces<WorkoutPlanResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{workoutPlanId}/weeks/{week}/generate", GenerateWeek)
            .WithName("GenerateWeek")
            .Produces<WeeklyPlanResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateWorkoutPlan(
        CreateWorkoutPlanRequest request,
        ICommandHandler<CreateWorkoutPlanCommand, WorkoutPlanDto> handler,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        // Map request to command
        var command = mapper.Map<CreateWorkoutPlanCommand>(request);

        // Execute command
        var result = await handler.HandleAsync(command, cancellationToken);

        // Map result to response
        var response = mapper.Map<WorkoutPlanResponse>(result);

        return Results.Created($"/api/workout-plans/{response.Id}", response);
    }

    private static async Task<IResult> CompleteWorkoutDay(
        Guid workoutPlanId,
        CompleteWorkoutDayRequest request,
        ICommandHandler<CompleteWorkoutDayCommand, CompleteDayResultDto> handler,
        CancellationToken cancellationToken)
    {
        var command = new CompleteWorkoutDayCommand(
            workoutPlanId,
            request.ExercisePerformances);

        var result = await handler.HandleAsync(command, cancellationToken);

        return Results.Ok(new CompleteDayResponse
        {
            WorkoutPlanId = result.WorkoutPlanId,
            CompletedWeek = result.CompletedWeek,
            CompletedDay = result.CompletedDay,
            NewWeek = result.NewWeek,
            NewDay = result.NewDay,
            WeekCompleted = result.WeekCompleted
        });
    }
}
```

## Request DTOs

**Rules:**
1. **Validation attributes:** Use DataAnnotations for basic validation
2. **Records preferred:** Use records for immutability
3. **No business logic:** Pure data structures
4. **Separate from Application:** Don't reuse Application DTOs

**Pattern:**
```csharp
public sealed record CreateWorkoutPlanRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string WorkoutName { get; init; } = string.Empty;

    [Required]
    [Range(1, 7)]
    public int WorkoutsPerWeek { get; init; }

    [Required]
    [MinLength(1)]
    public List<CreateExerciseRequest> Exercises { get; init; } = new();
}

public sealed record CreateExerciseRequest
{
    [Required]
    [StringLength(200)]
    public string ExerciseName { get; init; } = string.Empty;

    [Required]
    [Range(1, 7)]
    public int Day { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Order { get; init; }

    [Required]
    public ExerciseProgressionRequest Progression { get; init; } = null!;
}

// Polymorphic request using JsonPolymorphic
[JsonPolymorphic(TypeDiscriminatorPropertyName = "programType")]
[JsonDerivedType(typeof(HypertrophyProgressionRequest), "Hypertrophy")]
[JsonDerivedType(typeof(RepsPerSetProgressionRequest), "RepsPerSet")]
public abstract record ExerciseProgressionRequest
{
    [Required]
    public string ProgramType { get; init; } = string.Empty;
}

public sealed record HypertrophyProgressionRequest : ExerciseProgressionRequest
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TrainingMax { get; init; }

    [Required]
    [Range(0.01, 100)]
    public decimal WeightProgression { get; init; }

    public bool IsPrimary { get; init; }
}
```

## Response DTOs

**Pattern:**
```csharp
public sealed record WorkoutPlanResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public WorkoutActivityResponse Activity { get; init; } = null!;
    public List<ExerciseResponse> Exercises { get; init; } = new();
}

public sealed record ErrorResponse
{
    public string Message { get; init; } = string.Empty;
    public string? Detail { get; init; }
    public List<ValidationError> Errors { get; init; } = new();
}

public sealed record ValidationError
{
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
```

## Exception Handling Middleware

**Pattern:**
```csharp
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception occurred");
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed");
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }

    private static Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Message = "Business rule violation",
            Detail = ex.Message
        });
    }

    private static Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Message = "Resource not found",
            Detail = ex.Message
        });
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        return context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Message = "Validation failed",
            Errors = ex.Errors.Select(e => new ValidationError
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            }).ToList()
        });
    }

    private static Task HandleUnhandledExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Message = "An unexpected error occurred",
            Detail = "Please contact support if the problem persists"
        });
    }
}
```

## Program.cs Configuration

**Pattern:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "A2S Workout Tracker API",
        Version = "v1"
    });
});

// Add application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapWorkoutPlanEndpoints();

app.Run();
```

## Testing

**Integration Test Requirements:**
- Use WebApplicationFactory for in-memory testing
- Test all endpoints (happy path + error cases)
- Test authentication/authorization
- Test request validation
- Test HTTP status codes

**Test Pattern:**
```csharp
public class WorkoutPlanEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WorkoutPlanEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateWorkoutPlan_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateWorkoutPlanRequest
        {
            WorkoutName = "Test Workout",
            WorkoutsPerWeek = 5,
            Exercises = new List<CreateExerciseRequest> { /* ... */ }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/workout-plans", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<WorkoutPlanResponse>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Workout");
    }
}
```
```

---
