# HevySync A2S Migration App - DDD Architecture Review

**Date**: 2025-12-25
**Reviewer**: DDD Software Architect
**Purpose**: Architectural review of A2S spreadsheet to web app migration

---

## Executive Summary

The HevySync application is designed to migrate Greg Nuckols' Average to Savage (A2S) spreadsheet calculations to a web application. The system will:
1. Create workouts with A2S progression strategies
2. Calculate sets server-side using A2S formulas
3. Track workout completion and performance
4. Apply progressive overload based on weekly results
5. **[MISSING]** Integrate with Hevy app to create actual workouts

**Current Status**: ✅ Core domain model complete | ⚠️ Hevy integration incomplete | ❌ A2S import missing

---

## Table of Contents

1. [Bounded Contexts](#bounded-contexts)
2. [Aggregate Design Review](#aggregate-design-review)
3. [Current System Flows](#current-system-flows)
4. [Missing Components](#missing-components)
5. [DDD Violations & Improvements](#ddd-violations--improvements)
6. [Encapsulation Issues](#encapsulation-issues)
7. [Recommended Changes](#recommended-changes)

---

## Bounded Contexts

### Current Implementation: Single Bounded Context

```
┌─────────────────────────────────────────────────────────────────┐
│                    WORKOUT MANAGEMENT CONTEXT                    │
│                                                                  │
│  Responsibilities:                                               │
│  - Workout program creation and configuration                    │
│  - Exercise management with progression strategies               │
│  - Training session tracking and completion                      │
│  - Progressive overload calculation (A2S formulas)               │
│  - Weekly progression based on performance                       │
│                                                                  │
│  Language: Workout, Exercise, Progression, Set, Training Max     │
└─────────────────────────────────────────────────────────────────┘
```

### ⚠️ **Issue**: Missing Bounded Contexts

The current design treats everything as one context. A better approach:

```
┌──────────────────────────────────────────────────────────────────────┐
│                         PROPOSED ARCHITECTURE                         │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────┐       ┌─────────────────────────┐
│   PROGRAM DESIGN BC     │       │   TRAINING SESSION BC   │
│                         │       │                         │
│ - A2S Program Templates │◄──────│ - Session Execution     │
│ - Exercise Progression  │       │ - Performance Tracking  │
│ - Set Calculation       │       │ - Rest Timers           │
│ - Training Max          │       │ - Exercise Completion   │
└─────────────────────────┘       └─────────────────────────┘
            │                                  │
            │                                  │
            ▼                                  ▼
┌─────────────────────────┐       ┌─────────────────────────┐
│  HEVY INTEGRATION BC    │       │   USER MANAGEMENT BC    │
│                         │       │                         │
│ - Workout Sync          │       │ - Authentication        │
│ - Exercise Mapping      │       │ - User Preferences      │
│ - API Communication     │       │ - Subscription          │
└─────────────────────────┘       └─────────────────────────┘
```

**Recommendation**: Consider splitting when Hevy integration is added.

---

## Aggregate Design Review

### 1. Workout Aggregate

**File**: `HevySync.Domain/Aggregates/Workout.cs`

#### Current Structure

```
Workout (Aggregate Root)
├── Properties
│   ├── Id: Guid
│   ├── Name: WorkoutName (VO)
│   ├── UserId: Guid
│   ├── Activity: WorkoutActivity (VO) - Current week/day
│   └── Exercises: List<Exercise> (Collection of child entities)
│
├── Public Methods (Consistency Boundary)
│   ├── CompleteDay(performances[]) - Advance to next day
│   └── ApplyProgression(weekPerformances[]) - Update progression strategies
│
└── Domain Events
    ├── WorkoutCreatedEvent
    └── WorkoutDayCompletedEvent
```

#### 🔴 **Critical Issues**

**Issue #1: Exposed Setters Break Encapsulation**

```csharp
// Current implementation - VIOLATES ENCAPSULATION
public List<Exercise> Exercises { get; set; } = new(); // ❌ Public setter

// External code can do:
workout.Exercises.Add(new Exercise()); // Bypasses validation
workout.Exercises.Clear(); // Destroys aggregate consistency
workout.Exercises = null; // Breaks invariants
```

**Issue #2: Missing Factory Method**

```csharp
// Current code allows invalid creation
var workout = new Workout(); // No name, no exercises, invalid state
```

**Issue #3: Direct Exercise Manipulation**

Exercises can be modified outside aggregate root:
```csharp
var exercise = workout.Exercises.First();
exercise.Name = ExerciseName.Create("Hacked"); // No aggregate validation
```

**Issue #4: No Exercise Ordering Validation**

Day/Order combinations can be duplicated without validation.

**Issue #5: Activity Tracking is Unclear**

`WorkoutActivity` contains `Week` and `Day`, but:
- No validation that day exists in workout
- `WorkoutsInWeek` is redundant (can be calculated)
- Should track history, not just current position

---

### 2. Exercise Entity

**File**: `HevySync.Domain/Entities/Exercise.cs`

#### Current Structure

```
Exercise (Child Entity)
├── Properties
│   ├── Id: Guid
│   ├── Name: ExerciseName (VO)
│   ├── ExerciseTemplateId: string (Hevy reference)
│   ├── RestTimer: RestTimer (VO)
│   ├── Day: int
│   ├── Order: int
│   ├── NumberOfSets: int
│   ├── WorkoutId: Guid
│   ├── BodyCategory: BodyCategory?
│   ├── EquipmentType: EquipmentType?
│   └── Progression: ExerciseProgression (1:1)
│
└── No Methods (Anemic Domain Model)
```

#### 🔴 **Critical Issues**

**Issue #1: Anemic Domain Model**

Exercise has no behavior, just properties. It's a data bag.

```csharp
// Current - no behavior
public class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; set; } // Just getters/setters
    public int NumberOfSets { get; set; }
}

// Should have:
public void UpdateSetCount(int sets) { /* validation */ }
public void AdjustRestTimer(RestTimer timer) { /* validation */ }
```

**Issue #2: Exposed Progression**

```csharp
public ExerciseProgression Progression { get; set; } // ❌ Can be replaced externally

// External code can do:
exercise.Progression = new RepsPerSetStrategy(...); // No validation
```

**Issue #3: No Validation Methods**

Day/Order validation should be on Exercise, not Workout.

---

### 3. ExerciseProgression Hierarchy

**File**: `HevySync.Domain/Entities/ExerciseProgression.cs`

#### Current Structure

```
ExerciseProgression (Abstract Base)
├── LinearProgressionStrategy
│   ├── TrainingMax: TrainingMax (VO)
│   ├── WeightProgression: WeightProgression (VO)
│   ├── AttemptsBeforeDeload: int
│   ├── IsPrimary: bool
│   └── ProgramType: LinearProgression enum
│
└── RepsPerSetStrategy
    ├── RepRange: RepRange (VO)
    ├── StartingSetCount: int
    ├── TargetSetCount: int
    └── StartingWeight: decimal
```

#### ✅ **Good Design**

- Strategy pattern for different progression types
- Table-per-hierarchy persistence
- Each strategy has specific progression logic

#### 🟡 **Minor Issues**

**Issue #1: Missing Abstract Apply Method**

```csharp
// Should have:
public abstract void ApplyProgression(PerformanceResult result);
```

Currently, progression is applied externally in `Workout.ApplyProgression()`.

**Issue #2: No Self-Validation**

Each strategy should validate its invariants in constructor.

---

## Current System Flows

### Flow 1: Create A2S Workout Program

```
┌──────┐                                                          ┌──────────┐
│Client│                                                          │ Domain   │
└──┬───┘                                                          └────┬─────┘
   │                                                                    │
   │ POST /average2savage/workout                                      │
   │ {                                                                 │
   │   name: "A2S Hypertrophy 5x",                                     │
   │   userId: "...",                                                  │
   │   workoutsInWeek: 5,                                              │
   │   exercises: [...]                                                │
   │ }                                                                 │
   ├─────────────────────────────────────────────────────────────────►│
   │                                                                   │
   │                        Application Layer                         │
   │                   CreateWorkoutCommandHandler                    │
   │                               │                                  │
   │                               │ 1. Validate Request              │
   │                               ├──────────────────────────────┐   │
   │                               │                              │   │
   │                               │ 2. Map DTOs to Domain        │   │
   │                               │    - WorkoutName VO          │   │
   │                               │    - Exercise entities       │   │
   │                               │    - Progression strategies  │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 3. Create Workout Aggregate      │
   │                               ├──────────────────────────────┐   │
   │                               │  new Workout(name, userId)   │   │
   │                               │  Raises: WorkoutCreatedEvent │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 4. Persist via Repository        │
   │                               ├──────────────────────────────┐   │
   │                               │  _repository.Add(workout)    │   │
   │                               │  _unitOfWork.SaveChanges()   │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │◄──────────────────────────────┤                                  │
   │ WorkoutDto { id, name, ... }                                     │
   │                                                                   │
```

**Result**: Workout stored with exercises, no sets generated yet.

---

### Flow 2: Generate Week 1 Sets (A2S Calculations)

```
┌──────┐                                                          ┌──────────┐
│Client│                                                          │ Domain   │
└──┬───┘                                                          └────┬─────┘
   │                                                                    │
   │ POST /average2savage/workout/create-week-one                      │
   │ { workoutId: "..." }                                              │
   ├─────────────────────────────────────────────────────────────────►│
   │                                                                   │
   │                        Application Layer                         │
   │                  GenerateWeekOneCommandHandler                   │
   │                               │                                  │
   │                               │ 1. Load Workout + Exercises      │
   │                               ├──────────────────────────────┐   │
   │                               │  Include: Exercises           │   │
   │                               │          .Progression         │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 2. Group by Day                  │
   │                               ├──────────────────────────────┐   │
   │                               │  Day 1: [Ex1, Ex2, Ex3]       │   │
   │                               │  Day 2: [Ex4, Ex5]            │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 3. For each exercise:            │
   │                               │                                  │
   │                               │ IF Linear Progression:           │
   │                               ├──────────────────────────────┐   │
   │                               │  ILinearProgressionCalculator │   │
   │                               │  .CalculateSets(              │   │
   │                               │    week: 1,                   │   │
   │                               │    trainingMax: 225,          │   │
   │                               │    isPrimary: true            │   │
   │                               │  )                            │   │
   │                               │                               │   │
   │                               │  Returns: [                   │   │
   │                               │    Set(180lb, 6 reps),        │   │
   │                               │    Set(180lb, 6 reps),        │   │
   │                               │    Set(180lb, 6 reps),        │   │
   │                               │    Set(180lb, 6+ reps AMRAP)  │   │
   │                               │  ]                            │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ IF Reps Per Set:                 │
   │                               ├──────────────────────────────┐   │
   │                               │  IRepsPerSetCalculator        │   │
   │                               │  .CalculateSets(              │   │
   │                               │    repRange: 8-12,            │   │
   │                               │    setCount: 3,               │   │
   │                               │    weight: 135                │   │
   │                               │  )                            │   │
   │                               │                               │   │
   │                               │  Returns: [                   │   │
   │                               │    Set(135lb, 8 reps),        │   │
   │                               │    Set(135lb, 8 reps),        │   │
   │                               │    Set(135lb, 8 reps)         │   │
   │                               │  ]                            │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │◄──────────────────────────────┤                                  │
   │ {                                                                 │
   │   1: [ // Day 1                                                   │
   │     { exerciseName: "Squat", sets: [...] },                       │
   │     { exerciseName: "Bench", sets: [...] }                        │
   │   ],                                                              │
   │   2: [ // Day 2                                                   │
   │     { exerciseName: "Deadlift", sets: [...] }                     │
   │   ]                                                               │
   │ }                                                                 │
```

**Key Point**: Sets are **calculated on-demand**, not stored. A2S formulas applied server-side.

---

### Flow 3: Complete Workout Day

```
┌──────┐                                                          ┌──────────┐
│Client│                                                          │ Domain   │
└──┬───┘                                                          └────┬─────┘
   │                                                                    │
   │ POST /average2savage/workout/complete-day                         │
   │ {                                                                 │
   │   workoutId: "...",                                               │
   │   performances: [                                                 │
   │     {                                                             │
   │       exerciseId: "...",                                          │
   │       sets: [                                                     │
   │         { weight: 180, reps: 6 },                                 │
   │         { weight: 180, reps: 6 },                                 │
   │         { weight: 180, reps: 8 } // AMRAP                         │
   │       ],                                                          │
   │       performanceResult: "Success"                                │
   │     }                                                             │
   │   ]                                                               │
   │ }                                                                 │
   ├─────────────────────────────────────────────────────────────────►│
   │                                                                   │
   │                        Application Layer                         │
   │                 CompleteWorkoutDayCommandHandler                 │
   │                               │                                  │
   │                               │ 1. Load Workout                  │
   │                               ├──────────────────────────────┐   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 2. Map to Domain VOs             │
   │                               ├──────────────────────────────┐   │
   │                               │  ExercisePerformance VOs      │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 3. Aggregate Method              │
   │                               ├──────────────────────────────┐   │
   │                               │  workout.CompleteDay(         │   │
   │                               │    performances               │   │
   │                               │  )                            │   │
   │                               │                               │   │
   │                               │  Inside:                      │   │
   │                               │  - Validate all exercises     │   │
   │                               │    have performance data      │   │
   │                               │  - Raise                      │   │
   │                               │    WorkoutDayCompletedEvent   │   │
   │                               │  - Increment Day              │   │
   │                               │    (or Week if day > max)     │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 4. Persist Changes               │
   │                               ├──────────────────────────────┐   │
   │                               │  _repository.Update(workout)  │   │
   │                               │  _unitOfWork.SaveChanges()    │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │◄──────────────────────────────┤                                  │
   │ {                                                                 │
   │   currentWeek: 1,                                                 │
   │   currentDay: 2,                                                  │
   │   message: "Day completed. Next: Week 1, Day 2"                   │
   │ }                                                                 │
```

**Result**: Workout advances to next day, domain event published.

---

### Flow 4: Generate Next Week (Apply A2S Progression)

```
┌──────┐                                                          ┌──────────┐
│Client│                                                          │ Domain   │
└──┬───┘                                                          └────┬─────┘
   │                                                                    │
   │ POST /average2savage/workout/generate-next-week                   │
   │ {                                                                 │
   │   workoutId: "...",                                               │
   │   weekPerformances: [                                             │
   │     {                                                             │
   │       exerciseId: "...",                                          │
   │       exerciseName: "Squat",                                      │
   │       day: 1,                                                     │
   │       performanceResult: "Success" // Hit AMRAP target            │
   │     },                                                            │
   │     {                                                             │
   │       exerciseId: "...",                                          │
   │       exerciseName: "Bench",                                      │
   │       day: 2,                                                     │
   │       performanceResult: "Failed" // Missed reps                  │
   │     }                                                             │
   │   ]                                                               │
   │ }                                                                 │
   ├─────────────────────────────────────────────────────────────────►│
   │                                                                   │
   │                        Application Layer                         │
   │                 GenerateNextWeekCommandHandler                   │
   │                               │                                  │
   │                               │ 1. Load Workout + Exercises      │
   │                               ├──────────────────────────────┐   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 2. Aggregate Method              │
   │                               ├──────────────────────────────┐   │
   │                               │  workout.ApplyProgression(    │   │
   │                               │    weekPerformances           │   │
   │                               │  )                            │   │
   │                               │                               │   │
   │                               │  Inside:                      │   │
   │                               │  For each exercise:           │   │
   │                               │                               │   │
   │                               │  IF LinearProgression:        │   │
   │                               │    IF Success:                │   │
   │                               │      trainingMax += 5lb       │   │
   │                               │    IF Failed:                 │   │
   │                               │      trainingMax -= 10lb      │   │
   │                               │                               │   │
   │                               │  IF RepsPerSet:               │   │
   │                               │    IF Success:                │   │
   │                               │      IF sets < target:        │   │
   │                               │        sets += 1              │   │
   │                               │      ELSE:                    │   │
   │                               │        weight += 5lb          │   │
   │                               │        sets = starting        │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 3. Persist Updated Workout       │
   │                               ├──────────────────────────────┐   │
   │                               │  _repository.Update(workout)  │   │
   │                               │  _unitOfWork.SaveChanges()    │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │                               │ 4. Generate Sets for New Week    │
   │                               ├──────────────────────────────┐   │
   │                               │  Same as Flow 2, but with     │   │
   │                               │  updated progression values   │   │
   │                               │◄─────────────────────────────┘   │
   │                               │                                  │
   │◄──────────────────────────────┤                                  │
   │ {                                                                 │
   │   1: [ // Day 1 with NEW weights                                  │
   │     { exerciseName: "Squat", sets: [                              │
   │       Set(185lb, 6), // Increased due to success                  │
   │       Set(185lb, 6),                                              │
   │       Set(185lb, 6+)                                              │
   │     ]},                                                           │
   │     { exerciseName: "Bench", sets: [                              │
   │       Set(175lb, 6), // Decreased due to failure                  │
   │       ...                                                         │
   │     ]}                                                            │
   │   ]                                                               │
   │ }                                                                 │
```

**Key Point**: A2S progression rules applied to update exercise parameters.

---

## Missing Components

### 🔴 **Critical Missing: Hevy App Integration**

**Status**: Partially implemented but incomplete

**Existing Code**:
- `HevySync/Endpoints/Hevy/GetWorkout.cs` - Endpoint exists
- `HevySync.Application/Workouts/Queries/GetWorkout/` - Query handler exists
- DTOs defined

**Missing**:
1. **Hevy API Client**
   - No HTTP client to communicate with Hevy API
   - No authentication/authorization for Hevy
   - No workout creation on Hevy side

2. **Exercise Template Mapping**
   - `ExerciseTemplateId` exists on Exercise entity
   - No service to map A2S exercises to Hevy exercise templates
   - No validation that Hevy templates exist

3. **Workout Sync Service**
   - No service to push generated week to Hevy app
   - No tracking of sync status (synced vs. pending)
   - No retry logic for failed syncs

4. **Domain Event Handlers**
   - `WorkoutCreatedEvent` is raised but not handled
   - Should trigger Hevy workout creation
   - No compensation logic if Hevy sync fails

**Proposed Architecture**:

```
Domain Event: WorkoutCreatedEvent
              │
              ▼
   Application Layer Handler
   WorkoutCreatedEventHandler
              │
              ├─────► Map Exercises to Hevy Templates
              │         (IExerciseMappingService)
              │
              ├─────► Create Workout on Hevy
              │         (IHevyApiClient)
              │
              └─────► Update Workout with Hevy ID
                        (Aggregate: workout.MarkSyncedWithHevy(hevyId))
```

---

### 🟡 **Medium Priority: A2S Spreadsheet Import**

**Status**: Not implemented

**Requirement**: Users should be able to import existing A2S spreadsheet to migrate to web app.

**Missing**:
1. **File Upload Endpoint**
   - Accept `.xlsx` file upload
   - Parse spreadsheet structure

2. **A2S Parser Service**
   - Read exercise names from spreadsheet
   - Extract training maxes
   - Identify program type (Hypertrophy, Strength)
   - Map to domain model

3. **Import Command**
   - `ImportA2SSpreadsheetCommand`
   - Validate spreadsheet format
   - Create Workout aggregate from parsed data

**Example Flow**:

```
User uploads "A2S_Hypertrophy.xlsx"
        │
        ▼
ImportA2SSpreadsheetCommandHandler
        │
        ├─── Parse spreadsheet
        │    ├─ Sheet: "Setup" → Extract program config
        │    ├─ Sheet: "Week 1" → Extract exercises
        │    └─ Sheet: "Maxes" → Extract training maxes
        │
        ├─── Map to CreateWorkoutCommand
        │    └─ Convert rows to Exercise DTOs
        │
        └─── Execute CreateWorkoutCommand
             └─ Creates Workout aggregate
```

---

### 🟡 **Medium Priority: Performance History Tracking**

**Status**: Performance recorded but not persisted

**Issue**: `CompleteWorkoutDay` receives performance data but doesn't store it.

**Missing**:
1. **PerformanceHistory Aggregate**
   - Track all completed workout days
   - Store actual sets performed
   - Enable progress visualization

2. **Performance Repository**
   - Persist performance records
   - Query performance trends

3. **Performance Queries**
   - `GetExerciseProgressQuery` - Show weight/reps over time
   - `GetWorkoutHistoryQuery` - Past workout summaries

**Proposed Entity**:

```csharp
public class WorkoutDayPerformance : Entity<Guid>
{
    public Guid WorkoutId { get; private set; }
    public int Week { get; private set; }
    public int Day { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public List<ExercisePerformanceRecord> Exercises { get; private set; }
}

public class ExercisePerformanceRecord : Entity<Guid>
{
    public Guid ExerciseId { get; private set; }
    public ExerciseName Name { get; private set; }
    public List<Set> ActualSets { get; private set; } // What user did
    public List<Set> PrescribedSets { get; private set; } // What was planned
    public PerformanceResult Result { get; private set; }
}
```

---

### 🟢 **Low Priority: Deload Weeks**

**Status**: Logic partially exists

**Issue**: `AttemptsBeforeDeload` exists on `LinearProgressionStrategy` but not used.

**Missing**:
1. **Deload Tracking**
   - Count consecutive failed weeks
   - Trigger deload automatically

2. **Deload Calculation**
   - Reduce training max by 10%
   - Return to easier week (e.g., Week 2 intensity)

---

### 🟢 **Low Priority: User Preferences**

**Status**: User ID exists but no user context

**Missing**:
1. **User Entity**
   - Preferences (units: kg/lb)
   - Default rest timers
   - Program preferences

2. **Unit Conversion**
   - Convert kg ↔ lb for international users

---

## DDD Violations & Improvements

### Violation #1: Anemic Domain Model (Exercise)

**Severity**: 🔴 Critical

**Current**:
```csharp
public class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; set; } // Just data
    public int NumberOfSets { get; set; }
    public RestTimer RestTimer { get; set; }
    public int Day { get; set; }
    public int Order { get; set; }
}

// External manipulation
exercise.Day = 99; // Invalid, no validation
exercise.NumberOfSets = -1; // Invalid
```

**Fix**:
```csharp
public class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; private set; } // Private setters
    public int NumberOfSets { get; private set; }
    public RestTimer RestTimer { get; private set; }
    public int Day { get; private set; }
    public int Order { get; private set; }

    // Factory method
    public static Exercise Create(
        ExerciseName name,
        int day,
        int order,
        int numberOfSets,
        RestTimer restTimer,
        ExerciseProgression progression)
    {
        if (day < 1 || day > 7)
            throw new DomainException("Day must be 1-7");
        if (numberOfSets < 1)
            throw new DomainException("Must have at least 1 set");

        return new Exercise
        {
            Id = Guid.NewGuid(),
            Name = name,
            Day = day,
            Order = order,
            NumberOfSets = numberOfSets,
            RestTimer = restTimer,
            Progression = progression
        };
    }

    // Behavior methods
    public void UpdateRestTimer(RestTimer newTimer)
    {
        RestTimer = newTimer;
    }

    public void UpdateSetCount(int sets)
    {
        if (sets < 1)
            throw new DomainException("Must have at least 1 set");
        NumberOfSets = sets;
    }

    public void Reorder(int newOrder)
    {
        if (newOrder < 0)
            throw new DomainException("Order must be non-negative");
        Order = newOrder;
    }
}
```

---

### Violation #2: Exposed Collections on Aggregate Root

**Severity**: 🔴 Critical

**Current**:
```csharp
public class Workout : AggregateRoot<Guid>
{
    public List<Exercise> Exercises { get; set; } = new(); // ❌
}

// Allows:
workout.Exercises.Clear(); // Bypasses aggregate
workout.Exercises.Add(invalidExercise);
```

**Fix**:
```csharp
public class Workout : AggregateRoot<Guid>
{
    private readonly List<Exercise> _exercises = new();

    // Read-only access
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    // Controlled mutation through aggregate
    public void AddExercise(
        ExerciseName name,
        int day,
        int order,
        int numberOfSets,
        RestTimer restTimer,
        ExerciseProgression progression)
    {
        // Validation
        if (_exercises.Any(e => e.Day == day && e.Order == order))
            throw new DomainException($"Exercise already exists at Day {day}, Order {order}");

        var exercise = Exercise.Create(name, day, order, numberOfSets, restTimer, progression);
        _exercises.Add(exercise);
    }

    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new DomainException("Exercise not found");

        _exercises.Remove(exercise);
    }

    public void ReorderExercise(Guid exerciseId, int newOrder)
    {
        var exercise = GetExercise(exerciseId);

        // Check for conflicts
        if (_exercises.Any(e => e.Day == exercise.Day && e.Order == newOrder && e.Id != exerciseId))
            throw new DomainException("Order conflict");

        exercise.Reorder(newOrder);
    }

    private Exercise GetExercise(Guid id)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == id);
        if (exercise == null)
            throw new DomainException("Exercise not found");
        return exercise;
    }
}
```

---

### Violation #3: Missing Factory Method on Aggregate Root

**Severity**: 🔴 Critical

**Current**:
```csharp
// Can create invalid aggregate
var workout = new Workout(); // No name, no user, invalid state
```

**Fix**:
```csharp
public class Workout : AggregateRoot<Guid>
{
    // Private constructor
    private Workout() { }

    // Factory method
    public static Workout Create(
        WorkoutName name,
        Guid userId,
        int workoutsInWeek)
    {
        if (workoutsInWeek < 1 || workoutsInWeek > 7)
            throw new DomainException("Workouts in week must be 1-7");

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            Activity = WorkoutActivity.Create(1, 1, workoutsInWeek),
            _exercises = new List<Exercise>()
        };

        workout.AddDomainEvent(new WorkoutCreatedEvent(
            workout.Id,
            workout.Name,
            workout.UserId,
            DateTime.UtcNow
        ));

        return workout;
    }
}

// Application layer:
var workout = Workout.Create(
    WorkoutName.Create("A2S Hypertrophy"),
    userId,
    workoutsInWeek: 5
);
```

---

### Violation #4: Progression Applied Outside Aggregate

**Severity**: 🟡 Medium

**Current**:
```csharp
// In Workout.ApplyProgression():
var exercise = _exercises.First(e => e.Id == perf.ExerciseId);

if (exercise.Progression is LinearProgressionStrategy linear)
{
    // Direct mutation of progression
    linear.TrainingMax = TrainingMax.Create(newMax);
}
```

**Issue**: Progression strategies should update themselves.

**Fix**:
```csharp
public abstract class ExerciseProgression : Entity<Guid>
{
    // Template method
    public abstract void ApplyProgression(PerformanceResult result);
}

public class LinearProgressionStrategy : ExerciseProgression
{
    public override void ApplyProgression(PerformanceResult result)
    {
        var currentMax = TrainingMax.Value;
        var progression = WeightProgression.Value;

        var newMax = result switch
        {
            PerformanceResult.Success => currentMax + progression,
            PerformanceResult.Maintained => currentMax,
            PerformanceResult.Failed => currentMax - (progression * 2),
            _ => throw new ArgumentException("Invalid performance result")
        };

        TrainingMax = TrainingMax.Create(newMax);
    }
}

public class RepsPerSetStrategy : ExerciseProgression
{
    public override void ApplyProgression(PerformanceResult result)
    {
        if (result != PerformanceResult.Success)
            return; // No regression for RPS

        if (CurrentSetCount < TargetSetCount)
        {
            CurrentSetCount++;
        }
        else
        {
            // Increase weight, reset sets
            StartingWeight += 5m;
            CurrentSetCount = StartingSetCount;
        }
    }
}

// In Workout:
public void ApplyProgression(IEnumerable<WeekPerformance> weekPerformances)
{
    foreach (var perf in weekPerformances)
    {
        var exercise = GetExercise(perf.ExerciseId);
        exercise.Progression.ApplyProgression(perf.PerformanceResult);
    }
}
```

---

### Violation #5: No Validation in CompleteDay

**Severity**: 🟡 Medium

**Current**:
```csharp
public void CompleteDay(IEnumerable<ExercisePerformance> performances)
{
    // Validates all exercises have performance
    // But doesn't validate:
    // - Are exercises for current day?
    // - Is day already completed?
    // - Are exercise IDs valid?
}
```

**Fix**:
```csharp
public void CompleteDay(IEnumerable<ExercisePerformance> performances)
{
    var currentDay = Activity.Day;
    var exercisesForDay = _exercises.Where(e => e.Day == currentDay).ToList();

    // Validate day not already completed (requires history tracking)
    if (IsAlreadyCompleted(Activity.Week, currentDay))
        throw new DomainException("Day already completed");

    // Validate all exercises for current day have performance
    var performedExerciseIds = performances.Select(p => p.ExerciseId).ToHashSet();
    var missingExercises = exercisesForDay
        .Where(e => !performedExerciseIds.Contains(e.Id))
        .ToList();

    if (missingExercises.Any())
        throw new DomainException($"Missing performance for exercises: {string.Join(", ", missingExercises.Select(e => e.Name))}");

    // Validate no extra exercises
    var validExerciseIds = exercisesForDay.Select(e => e.Id).ToHashSet();
    var invalidPerformances = performances
        .Where(p => !validExerciseIds.Contains(p.ExerciseId))
        .ToList();

    if (invalidPerformances.Any())
        throw new DomainException("Performance data contains invalid exercise IDs");

    // Raise event
    AddDomainEvent(new WorkoutDayCompletedEvent(
        Id,
        Activity.Week,
        currentDay,
        performances,
        DateTime.UtcNow
    ));

    // Advance day
    Activity = Activity.AdvanceDay();
}
```

---

### Violation #6: Insufficient Value Object Validation

**Severity**: 🟢 Low

**Current**:
```csharp
public static WorkoutName Create(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Workout name cannot be empty");
    if (name.Length > 200)
        throw new ArgumentException("Workout name too long");

    return new WorkoutName { Value = name };
}
```

**Improvement**:
```csharp
public static WorkoutName Create(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException("Workout name cannot be empty", nameof(name));

    var trimmedName = name.Trim();

    if (trimmedName.Length > 200)
        throw new ArgumentException($"Workout name too long: {trimmedName.Length} characters (max 200)", nameof(name));

    // Additional validations
    if (trimmedName.Any(c => char.IsControl(c)))
        throw new ArgumentException("Workout name contains invalid characters", nameof(name));

    return new WorkoutName { Value = trimmedName };
}
```

---

## Encapsulation Issues

### Issue #1: Public Setters on Aggregate Root

**Files**: `Workout.cs`

**Current**:
```csharp
public WorkoutName Name { get; set; }
public Guid UserId { get; set; }
public WorkoutActivity Activity { get; set; }
public List<Exercise> Exercises { get; set; }
```

**Recommendation**:
```csharp
public WorkoutName Name { get; private set; }
public Guid UserId { get; private set; }
public WorkoutActivity Activity { get; private set; }
private readonly List<Exercise> _exercises = new();
public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();
```

**Impact**: Prevents external code from violating aggregate invariants.

---

### Issue #2: Public Setters on Entities

**Files**: `Exercise.cs`, `ExerciseProgression.cs`, `LinearProgressionStrategy.cs`, `RepsPerSetStrategy.cs`

**Current**: All properties have public setters.

**Recommendation**: Make all setters `private set`, add behavior methods.

---

### Issue #3: Domain Logic in Application Layer

**File**: `GenerateNextWeekCommandHandler.cs`

**Current**:
```csharp
// Application layer knows about progression logic
var weekSets = new Dictionary<int, List<SessionExerciseDto>>();
for (int day = 1; day <= workout.Activity.WorkoutsInWeek; day++)
{
    var exercisesForDay = workout.Exercises
        .Where(e => e.Day == day)
        .OrderBy(e => e.Order)
        .ToList();

    var daySets = new List<SessionExerciseDto>();
    foreach (var exercise in exercisesForDay)
    {
        var sets = await _setGenerationService.GenerateSets(...);
        // ...
    }
}
```

**Issue**: Application layer knows about exercise ordering, days, set generation.

**Recommendation**: Move to aggregate.

```csharp
// On Workout aggregate:
public Dictionary<int, List<ExerciseWithSets>> GenerateSetsForCurrentWeek(
    ISetGenerationService setGenerationService)
{
    var result = new Dictionary<int, List<ExerciseWithSets>>();

    for (int day = 1; day <= Activity.WorkoutsInWeek; day++)
    {
        var exercisesForDay = GetExercisesForDay(day);
        var exerciseSets = new List<ExerciseWithSets>();

        foreach (var exercise in exercisesForDay)
        {
            var sets = setGenerationService.GenerateSets(
                exercise,
                Activity.Week
            );
            exerciseSets.Add(new ExerciseWithSets(exercise, sets));
        }

        result[day] = exerciseSets;
    }

    return result;
}

private IEnumerable<Exercise> GetExercisesForDay(int day)
{
    return _exercises
        .Where(e => e.Day == day)
        .OrderBy(e => e.Order);
}
```

---

### Issue #4: Fat DTOs with Validation Logic

**File**: `CreateWorkoutRequest.cs`

**Current**:
```csharp
public record CreateWorkoutRequest(
    string Name,
    Guid UserId,
    int WorkoutsInWeek,
    List<ExerciseDto> Exercises
);

public class CreateWorkoutRequestValidator : AbstractValidator<CreateWorkoutRequest>
{
    // Validation duplicates domain rules
}
```

**Issue**: Validation exists in both DTO validator AND domain objects.

**Recommendation**:
- DTO validators: Basic type checking (not null, valid format)
- Domain objects: Business rule validation

---

## Recommended Changes

### Priority 1: Fix Encapsulation (Critical)

**Changes**:
1. ✅ Make all aggregate/entity setters private
2. ✅ Add factory methods to Workout and Exercise
3. ✅ Protect Exercises collection with IReadOnlyCollection
4. ✅ Add behavior methods to Exercise
5. ✅ Move progression logic to ExerciseProgression.ApplyProgression()

**Estimated Effort**: 4-6 hours
**Impact**: Prevents 90% of potential invariant violations

---

### Priority 2: Add Hevy Integration (Critical for Product)

**Changes**:
1. ⬜ Create `IHevyApiClient` interface and implementation
2. ⬜ Create `IExerciseMappingService` to map A2S exercises to Hevy templates
3. ⬜ Add `WorkoutCreatedEventHandler` to sync with Hevy
4. ⬜ Add `HevyWorkoutId` to Workout aggregate
5. ⬜ Add `MarkSyncedWithHevy(string hevyWorkoutId)` method to Workout
6. ⬜ Add retry logic for failed syncs
7. ⬜ Add `SyncStatus` enum (NotSynced, Syncing, Synced, Failed)

**Estimated Effort**: 12-16 hours
**Impact**: Core product feature - migrating to Hevy app

---

### Priority 3: Add Performance History (Medium)

**Changes**:
1. ⬜ Create `WorkoutDayPerformance` aggregate
2. ⬜ Create `IPerformanceRepository`
3. ⬜ Add handler for `WorkoutDayCompletedEvent` to persist performance
4. ⬜ Add queries: `GetExerciseProgressQuery`, `GetWorkoutHistoryQuery`

**Estimated Effort**: 8-10 hours
**Impact**: Enables progress tracking and visualization

---

### Priority 4: Add A2S Import (Medium)

**Changes**:
1. ⬜ Add file upload endpoint
2. ⬜ Create `IA2SSpreadsheetParser` service
3. ⬜ Add `ImportA2SSpreadsheetCommand` and handler
4. ⬜ Map spreadsheet data to domain model

**Estimated Effort**: 10-12 hours
**Impact**: User onboarding - import existing programs

---

### Priority 5: Split Bounded Contexts (Low - Future)

**When to do**: When Hevy integration is complete and codebase grows.

**Proposed Contexts**:
1. **Program Design** - A2S templates, progression calculations
2. **Training Session** - Workout execution, performance tracking
3. **Hevy Integration** - External API, exercise mapping, sync
4. **User Management** - Auth, preferences, subscriptions

**Estimated Effort**: 20-30 hours (major refactor)
**Impact**: Better separation of concerns, team scalability

---

## Summary

### ✅ What's Working Well

1. **Clear Domain Language**: Workout, Exercise, Progression, Training Max - all understandable
2. **Value Objects**: Good use for WorkoutName, RestTimer, Set, etc.
3. **Polymorphic Progression**: Strategy pattern for Linear vs. Reps Per Set
4. **Domain Events**: WorkoutCreatedEvent and WorkoutDayCompletedEvent
5. **CQRS Separation**: Commands vs. Queries clearly separated
6. **A2S Calculations**: Server-side set generation with proper formulas

### 🔴 Critical Issues

1. **Broken Encapsulation**: Public setters everywhere, exposed collections
2. **Anemic Domain Model**: Entities lack behavior, just data bags
3. **Missing Hevy Integration**: Core product feature not implemented
4. **No Performance History**: Can't track progress over time
5. **Progression Logic Location**: Should be on entities, not in aggregate root

### 📋 Next Steps

**Immediate (This Week)**:
1. Fix encapsulation - make setters private
2. Add factory methods to Workout and Exercise
3. Protect Exercises collection
4. Move progression logic to ExerciseProgression.ApplyProgression()

**Short-term (Next 2 Weeks)**:
1. Implement Hevy API client
2. Add exercise mapping service
3. Create WorkoutCreatedEventHandler for sync
4. Add sync status tracking

**Medium-term (Next Month)**:
1. Add performance history aggregate
2. Implement A2S spreadsheet import
3. Add progress visualization queries

**Long-term (Next Quarter)**:
1. Consider splitting bounded contexts
2. Add deload week automation
3. Add user preferences and settings

---

## Architecture Diagrams

### Current Aggregate Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                    Workout (Aggregate Root)                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Properties                                                  │ │
│  │ - Id: Guid                                                  │ │
│  │ - Name: WorkoutName (VO)                                    │ │
│  │ - UserId: Guid                                              │ │
│  │ - Activity: WorkoutActivity (VO) - Owned Type               │ │
│  │   ├─ Week: int                                              │ │
│  │   ├─ Day: int                                               │ │
│  │   └─ WorkoutsInWeek: int                                    │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Exercises Collection (1..*)                                 │ │
│  │                                                             │ │
│  │  ┌──────────────────────────────────────────────────────┐  │ │
│  │  │ Exercise (Entity)                                     │  │ │
│  │  │ - Id: Guid                                            │  │ │
│  │  │ - Name: ExerciseName (VO)                             │  │ │
│  │  │ - ExerciseTemplateId: string                          │  │ │
│  │  │ - RestTimer: RestTimer (VO)                           │  │ │
│  │  │ - Day: int                                            │  │ │
│  │  │ - Order: int                                          │  │ │
│  │  │ - NumberOfSets: int                                   │  │ │
│  │  │ - BodyCategory: enum?                                 │  │ │
│  │  │ - EquipmentType: enum?                                │  │ │
│  │  │                                                        │  │ │
│  │  │  ┌─────────────────────────────────────────────────┐  │  │ │
│  │  │  │ Progression (Polymorphic Entity, TPH)           │  │  │ │
│  │  │  │                                                  │  │  │ │
│  │  │  │ LinearProgressionStrategy                        │  │  │ │
│  │  │  │ - TrainingMax: TrainingMax (VO)                  │  │  │ │
│  │  │  │ - WeightProgression: WeightProgression (VO)      │  │  │ │
│  │  │  │ - AttemptsBeforeDeload: int                      │  │  │ │
│  │  │  │ - IsPrimary: bool                                │  │  │ │
│  │  │  │ - ProgramType: enum                              │  │  │ │
│  │  │  │                                                  │  │  │ │
│  │  │  │ RepsPerSetStrategy                               │  │  │ │
│  │  │  │ - RepRange: RepRange (VO)                        │  │  │ │
│  │  │  │   ├─ MinimumReps: int                            │  │  │ │
│  │  │  │   ├─ TargetReps: int                             │  │  │ │
│  │  │  │   └─ MaximumTargetReps: int                      │  │  │ │
│  │  │  │ - StartingSetCount: int                          │  │  │ │
│  │  │  │ - TargetSetCount: int                            │  │  │ │
│  │  │  │ - StartingWeight: decimal                        │  │  │ │
│  │  │  └─────────────────────────────────────────────────┘  │  │ │
│  │  └──────────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Methods (Consistency Boundary)                              │ │
│  │ + CompleteDay(performances[])                               │ │
│  │ + ApplyProgression(weekPerformances[])                      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Domain Events                                               │ │
│  │ - WorkoutCreatedEvent                                       │ │
│  │ - WorkoutDayCompletedEvent                                  │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Proposed Bounded Context Map

```
┌─────────────────────────────────────────────────────────────────┐
│                         CONTEXT MAP                              │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────┐
│  PROGRAM DESIGN CONTEXT      │  ◄─── Core Domain
│                              │
│  Aggregates:                 │
│  - Workout                   │
│  - Exercise                  │
│  - ExerciseProgression       │
│                              │
│  Domain Services:            │
│  - LinearProgressionCalc     │
│  - RepsPerSetCalculator      │
│  - SetGenerationService      │
│                              │
│  Events Published:           │
│  - WorkoutCreatedEvent       │
└────────────┬─────────────────┘
             │
             │ Open Host Service (OHS)
             │ Publishes: WorkoutCreatedEvent
             │
             ▼
┌──────────────────────────────┐       ┌──────────────────────────┐
│ HEVY INTEGRATION CONTEXT     │       │ TRAINING SESSION CONTEXT │
│                              │       │                          │
│ Aggregates:                  │       │ Aggregates:              │
│ - HevyWorkout (external ref) │       │ - WorkoutDayPerformance  │
│ - ExerciseMapping            │       │                          │
│                              │       │ Value Objects:           │
│ Services:                    │       │ - ExercisePerformance    │
│ - IHevyApiClient             │       │ - Set                    │
│ - IExerciseMappingService    │       │ - PerformanceResult      │
│                              │       │                          │
│ Events Subscribed:           │       │ Events Subscribed:       │
│ - WorkoutCreatedEvent ◄──────┼───────┤ - WorkoutDayCompleted    │
│                              │       │                          │
│ Anti-Corruption Layer (ACL): │       │ Repository:              │
│ - Maps domain Workout to     │       │ - IPerformanceRepository │
│   Hevy API format            │       └──────────────────────────┘
└──────────────────────────────┘

┌──────────────────────────────┐
│ USER MANAGEMENT CONTEXT      │  ◄─── Supporting Subdomain
│                              │
│ Aggregates:                  │
│ - User                       │
│                              │
│ Value Objects:               │
│ - UserPreferences            │
│ - UnitSystem (kg/lb)         │
└──────────────────────────────┘

Relationships:
- Program Design → Hevy Integration: OHS/Published Language
- Program Design → Training Session: Shared Kernel (or Partnership)
- User Management → All: Customer/Supplier
```

---

**End of Architecture Review**
