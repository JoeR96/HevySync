# DDD Encapsulation Refactoring Guide

**Purpose**: Step-by-step guide to fix encapsulation violations and improve aggregate design

**Estimated Effort**: 4-6 hours

**Target**: Make aggregates properly protect their invariants through encapsulation

---

## Refactoring Checklist

### Phase 1: Workout Aggregate (2-3 hours)

- [ ] Add private constructor to Workout
- [ ] Create Workout.Create() factory method
- [ ] Make all property setters private
- [ ] Protect Exercises collection with IReadOnlyCollection
- [ ] Add AddExercise() method
- [ ] Add RemoveExercise() method
- [ ] Add GetExercise() helper method
- [ ] Add validation to CompleteDay()
- [ ] Add GenerateSetsForCurrentWeek() method
- [ ] Update EF Core configuration for private setters
- [ ] Update application layer to use factory method
- [ ] Update tests

### Phase 2: Exercise Entity (1-2 hours)

- [ ] Add private constructor to Exercise
- [ ] Create Exercise.Create() factory method
- [ ] Make all property setters private
- [ ] Add UpdateRestTimer() method
- [ ] Add UpdateSetCount() method
- [ ] Add Reorder() method
- [ ] Protect Progression with private setter
- [ ] Update EF Core configuration
- [ ] Update Workout aggregate to use factory
- [ ] Update tests

### Phase 3: ExerciseProgression (1 hour)

- [ ] Add abstract ApplyProgression() method to base class
- [ ] Implement ApplyProgression() in LinearProgressionStrategy
- [ ] Implement ApplyProgression() in RepsPerSetStrategy
- [ ] Make property setters private
- [ ] Add private constructors
- [ ] Add factory methods
- [ ] Update Workout.ApplyProgression() to delegate
- [ ] Update tests

### Phase 4: Application Layer Updates (1 hour)

- [ ] Update CreateWorkoutCommandHandler to use factory
- [ ] Update GenerateNextWeekCommandHandler to use aggregate method
- [ ] Remove domain logic from handlers
- [ ] Update integration tests
- [ ] Verify all endpoints still work

---

## Detailed Refactoring Steps

### Step 1: Refactor Workout Aggregate

**File**: `HevySync.Domain/Aggregates/Workout.cs`

#### Current Code (Problematic)

```csharp
public class Workout : AggregateRoot<Guid>
{
    public WorkoutName Name { get; set; }
    public Guid UserId { get; set; }
    public WorkoutActivity Activity { get; set; }
    public List<Exercise> Exercises { get; set; } = new();

    public void CompleteDay(IEnumerable<ExercisePerformance> performances)
    {
        // Minimal validation
        var performanceList = performances.ToList();
        var exerciseIds = Exercises.Select(e => e.Id).ToHashSet();
        var allExercisesHavePerformance = performanceList.All(p => exerciseIds.Contains(p.ExerciseId));

        if (!allExercisesHavePerformance)
            throw new InvalidOperationException("All exercises must have performance data.");

        AddDomainEvent(new WorkoutDayCompletedEvent(Id, Activity.Week, Activity.Day, performanceList, DateTime.UtcNow));
        Activity = Activity.AdvanceDay();
    }

    public void ApplyProgression(IEnumerable<WeekPerformance> weekPerformances)
    {
        // Long method with external logic...
    }
}
```

#### Refactored Code

```csharp
public class Workout : AggregateRoot<Guid>
{
    // Private constructor - force use of factory method
    private Workout()
    {
        _exercises = new List<Exercise>();
    }

    // Properties with private setters
    public WorkoutName Name { get; private set; }
    public Guid UserId { get; private set; }
    public WorkoutActivity Activity { get; private set; }

    // Protected collection
    private readonly List<Exercise> _exercises;
    public IReadOnlyCollection<Exercise> Exercises => _exercises.AsReadOnly();

    /// <summary>
    /// Factory method to create a new workout program.
    /// </summary>
    public static Workout Create(
        WorkoutName name,
        Guid userId,
        int workoutsInWeek)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID cannot be empty");

        if (workoutsInWeek < 1 || workoutsInWeek > 7)
            throw new DomainException("Workouts in week must be between 1 and 7");

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId,
            Activity = WorkoutActivity.Create(week: 1, day: 1, workoutsInWeek),
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

    /// <summary>
    /// Adds an exercise to the workout program.
    /// Validates day/order uniqueness.
    /// </summary>
    public void AddExercise(
        ExerciseName name,
        int day,
        int order,
        int numberOfSets,
        RestTimer restTimer,
        ExerciseProgression progression,
        string? exerciseTemplateId = null,
        BodyCategory? bodyCategory = null,
        EquipmentType? equipmentType = null)
    {
        // Validate day is within workout week
        if (day < 1 || day > Activity.WorkoutsInWeek)
            throw new DomainException($"Day must be between 1 and {Activity.WorkoutsInWeek}");

        // Check for duplicate day/order
        if (_exercises.Any(e => e.Day == day && e.Order == order))
            throw new DomainException($"Exercise already exists at Day {day}, Order {order}");

        var exercise = Exercise.Create(
            id: Guid.NewGuid(),
            workoutId: Id,
            name: name,
            day: day,
            order: order,
            numberOfSets: numberOfSets,
            restTimer: restTimer,
            progression: progression,
            exerciseTemplateId: exerciseTemplateId,
            bodyCategory: bodyCategory,
            equipmentType: equipmentType
        );

        _exercises.Add(exercise);
    }

    /// <summary>
    /// Removes an exercise from the workout.
    /// </summary>
    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new DomainException($"Exercise with ID {exerciseId} not found");

        _exercises.Remove(exercise);
    }

    /// <summary>
    /// Reorders an exercise within its day.
    /// </summary>
    public void ReorderExercise(Guid exerciseId, int newOrder)
    {
        var exercise = GetExercise(exerciseId);

        // Check if new order conflicts with existing exercise
        var conflict = _exercises.FirstOrDefault(e =>
            e.Day == exercise.Day &&
            e.Order == newOrder &&
            e.Id != exerciseId);

        if (conflict != null)
            throw new DomainException($"Order {newOrder} already occupied by exercise '{conflict.Name}' on day {exercise.Day}");

        exercise.Reorder(newOrder);
    }

    /// <summary>
    /// Completes a workout day with performance data.
    /// Validates all exercises have performance and advances to next day.
    /// </summary>
    public void CompleteDay(IEnumerable<ExercisePerformance> performances)
    {
        var performanceList = performances.ToList();
        var currentDay = Activity.Day;

        // Get exercises for current day
        var exercisesForDay = GetExercisesForDay(currentDay).ToList();

        if (!exercisesForDay.Any())
            throw new DomainException($"No exercises found for day {currentDay}");

        // Validate all exercises for current day have performance data
        var performedExerciseIds = performanceList.Select(p => p.ExerciseId).ToHashSet();
        var missingExercises = exercisesForDay
            .Where(e => !performedExerciseIds.Contains(e.Id))
            .ToList();

        if (missingExercises.Any())
        {
            var missingNames = string.Join(", ", missingExercises.Select(e => e.Name.Value));
            throw new DomainException($"Missing performance data for exercises: {missingNames}");
        }

        // Validate no extra exercises from other days
        var validExerciseIds = exercisesForDay.Select(e => e.Id).ToHashSet();
        var invalidPerformances = performanceList
            .Where(p => !validExerciseIds.Contains(p.ExerciseId))
            .ToList();

        if (invalidPerformances.Any())
            throw new DomainException($"Performance data contains {invalidPerformances.Count} exercise(s) not scheduled for day {currentDay}");

        // Raise domain event
        AddDomainEvent(new WorkoutDayCompletedEvent(
            Id,
            Activity.Week,
            currentDay,
            performanceList,
            DateTime.UtcNow
        ));

        // Advance to next day
        Activity = Activity.AdvanceDay();
    }

    /// <summary>
    /// Applies weekly progression based on performance results.
    /// Delegates to each exercise's progression strategy.
    /// </summary>
    public void ApplyProgression(IEnumerable<WeekPerformance> weekPerformances)
    {
        var performanceList = weekPerformances.ToList();

        foreach (var performance in performanceList)
        {
            var exercise = GetExercise(performance.ExerciseId);

            // Delegate to progression strategy
            exercise.Progression.ApplyProgression(performance.PerformanceResult);
        }
    }

    /// <summary>
    /// Generates sets for all exercises in the current week.
    /// Returns sets organized by day.
    /// </summary>
    public Dictionary<int, List<ExerciseWithSets>> GenerateSetsForCurrentWeek(
        ISetGenerationService setGenerationService)
    {
        if (setGenerationService == null)
            throw new ArgumentNullException(nameof(setGenerationService));

        var result = new Dictionary<int, List<ExerciseWithSets>>();

        for (int day = 1; day <= Activity.WorkoutsInWeek; day++)
        {
            var exercisesForDay = GetExercisesForDay(day).ToList();
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

    // Private helper methods
    private Exercise GetExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new DomainException($"Exercise with ID {exerciseId} not found in workout");
        return exercise;
    }

    private IEnumerable<Exercise> GetExercisesForDay(int day)
    {
        return _exercises
            .Where(e => e.Day == day)
            .OrderBy(e => e.Order);
    }
}

/// <summary>
/// Helper class to return exercise with generated sets.
/// </summary>
public record ExerciseWithSets(Exercise Exercise, IEnumerable<Set> Sets);
```

---

### Step 2: Refactor Exercise Entity

**File**: `HevySync.Domain/Entities/Exercise.cs`

#### Current Code

```csharp
public class Exercise : Entity<Guid>
{
    public ExerciseName Name { get; set; }
    public string? ExerciseTemplateId { get; set; }
    public RestTimer RestTimer { get; set; }
    public int Day { get; set; }
    public int Order { get; set; }
    public int NumberOfSets { get; set; }
    public Guid WorkoutId { get; set; }
    public BodyCategory? BodyCategory { get; set; }
    public EquipmentType? EquipmentType { get; set; }
    public ExerciseProgression Progression { get; set; }
}
```

#### Refactored Code

```csharp
public class Exercise : Entity<Guid>
{
    // Private constructor - force use of factory
    private Exercise() { }

    // Properties with private setters
    public ExerciseName Name { get; private set; }
    public string? ExerciseTemplateId { get; private set; }
    public RestTimer RestTimer { get; private set; }
    public int Day { get; private set; }
    public int Order { get; private set; }
    public int NumberOfSets { get; private set; }
    public Guid WorkoutId { get; private set; }
    public BodyCategory? BodyCategory { get; private set; }
    public EquipmentType? EquipmentType { get; private set; }

    // Protected progression - can only be set during creation
    public ExerciseProgression Progression { get; private set; }

    /// <summary>
    /// Factory method to create a new exercise.
    /// </summary>
    public static Exercise Create(
        Guid id,
        Guid workoutId,
        ExerciseName name,
        int day,
        int order,
        int numberOfSets,
        RestTimer restTimer,
        ExerciseProgression progression,
        string? exerciseTemplateId = null,
        BodyCategory? bodyCategory = null,
        EquipmentType? equipmentType = null)
    {
        if (id == Guid.Empty)
            throw new DomainException("Exercise ID cannot be empty");

        if (workoutId == Guid.Empty)
            throw new DomainException("Workout ID cannot be empty");

        if (day < 1)
            throw new DomainException("Day must be at least 1");

        if (order < 0)
            throw new DomainException("Order cannot be negative");

        if (numberOfSets < 1)
            throw new DomainException("Number of sets must be at least 1");

        if (progression == null)
            throw new DomainException("Progression strategy is required");

        return new Exercise
        {
            Id = id,
            WorkoutId = workoutId,
            Name = name,
            Day = day,
            Order = order,
            NumberOfSets = numberOfSets,
            RestTimer = restTimer,
            Progression = progression,
            ExerciseTemplateId = exerciseTemplateId,
            BodyCategory = bodyCategory,
            EquipmentType = equipmentType
        };
    }

    /// <summary>
    /// Updates the rest timer between sets.
    /// </summary>
    public void UpdateRestTimer(RestTimer newTimer)
    {
        if (newTimer == null)
            throw new ArgumentNullException(nameof(newTimer));

        RestTimer = newTimer;
    }

    /// <summary>
    /// Updates the number of sets for the exercise.
    /// </summary>
    public void UpdateSetCount(int sets)
    {
        if (sets < 1)
            throw new DomainException("Number of sets must be at least 1");

        NumberOfSets = sets;
    }

    /// <summary>
    /// Changes the order of the exercise within its day.
    /// Called by Workout aggregate after validation.
    /// </summary>
    internal void Reorder(int newOrder)
    {
        if (newOrder < 0)
            throw new DomainException("Order cannot be negative");

        Order = newOrder;
    }

    /// <summary>
    /// Updates the Hevy exercise template ID for syncing.
    /// </summary>
    public void MapToHevyTemplate(string hevyTemplateId)
    {
        if (string.IsNullOrWhiteSpace(hevyTemplateId))
            throw new DomainException("Hevy template ID cannot be empty");

        ExerciseTemplateId = hevyTemplateId;
    }
}
```

---

### Step 3: Refactor ExerciseProgression Hierarchy

**File**: `HevySync.Domain/Entities/ExerciseProgression.cs`

#### Current Base Class

```csharp
public abstract class ExerciseProgression : Entity<Guid>
{
    public Guid ExerciseId { get; set; }
}
```

#### Refactored Base Class

```csharp
public abstract class ExerciseProgression : Entity<Guid>
{
    protected ExerciseProgression() { } // For EF Core

    public Guid ExerciseId { get; protected set; }

    /// <summary>
    /// Applies progression based on weekly performance.
    /// Each strategy implements its own progression logic.
    /// </summary>
    public abstract void ApplyProgression(PerformanceResult result);

    /// <summary>
    /// Validates the progression strategy has valid parameters.
    /// </summary>
    protected abstract void ValidateInvariants();
}
```

---

**File**: `HevySync.Domain/Entities/LinearProgressionStrategy.cs`

#### Current Code

```csharp
public class LinearProgressionStrategy : ExerciseProgression
{
    public TrainingMax TrainingMax { get; set; }
    public WeightProgression WeightProgression { get; set; }
    public int AttemptsBeforeDeload { get; set; }
    public bool IsPrimary { get; set; }
    public LinearProgression ProgramType { get; set; }
}
```

#### Refactored Code

```csharp
public class LinearProgressionStrategy : ExerciseProgression
{
    private LinearProgressionStrategy() { } // For EF Core

    // Properties with private setters
    public TrainingMax TrainingMax { get; private set; }
    public WeightProgression WeightProgression { get; private set; }
    public int AttemptsBeforeDeload { get; private set; }
    public bool IsPrimary { get; private set; }
    public LinearProgression ProgramType { get; private set; }

    // Track deload attempts (future feature)
    private int _consecutiveFailures = 0;

    /// <summary>
    /// Factory method to create Linear Progression strategy.
    /// </summary>
    public static LinearProgressionStrategy Create(
        Guid exerciseId,
        TrainingMax trainingMax,
        WeightProgression weightProgression,
        bool isPrimary,
        LinearProgression programType = LinearProgression.AverageToSavageHypertrophy,
        int attemptsBeforeDeload = 3)
    {
        if (exerciseId == Guid.Empty)
            throw new DomainException("Exercise ID cannot be empty");

        if (attemptsBeforeDeload < 1)
            throw new DomainException("Attempts before deload must be at least 1");

        var strategy = new LinearProgressionStrategy
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            TrainingMax = trainingMax,
            WeightProgression = weightProgression,
            IsPrimary = isPrimary,
            ProgramType = programType,
            AttemptsBeforeDeload = attemptsBeforeDeload,
            _consecutiveFailures = 0
        };

        strategy.ValidateInvariants();
        return strategy;
    }

    /// <summary>
    /// Applies A2S linear progression rules.
    /// Success: Increase training max
    /// Maintained: No change
    /// Failed: Decrease training max
    /// </summary>
    public override void ApplyProgression(PerformanceResult result)
    {
        var currentMax = TrainingMax.Value;
        var progression = WeightProgression.Value;

        decimal newMax = result switch
        {
            PerformanceResult.Success => ApplySuccess(currentMax, progression),
            PerformanceResult.Maintained => currentMax,
            PerformanceResult.Failed => ApplyFailure(currentMax, progression),
            _ => throw new DomainException($"Unknown performance result: {result}")
        };

        TrainingMax = TrainingMax.Create(newMax);
    }

    private decimal ApplySuccess(decimal currentMax, decimal progression)
    {
        _consecutiveFailures = 0; // Reset failure count
        return currentMax + progression;
    }

    private decimal ApplyFailure(decimal currentMax, decimal progression)
    {
        _consecutiveFailures++;

        // Deload logic (future feature)
        if (_consecutiveFailures >= AttemptsBeforeDeload)
        {
            // Trigger deload week
            // For now, just decrease training max more aggressively
            _consecutiveFailures = 0;
            return currentMax - (progression * 2);
        }

        // Standard failure: decrease training max
        return currentMax - (progression * 2);
    }

    protected override void ValidateInvariants()
    {
        if (TrainingMax == null)
            throw new DomainException("Training Max is required");

        if (WeightProgression == null)
            throw new DomainException("Weight Progression is required");
    }
}
```

---

**File**: `HevySync.Domain/Entities/RepsPerSetStrategy.cs`

#### Current Code

```csharp
public class RepsPerSetStrategy : ExerciseProgression
{
    public RepRange RepRange { get; set; }
    public int StartingSetCount { get; set; }
    public int TargetSetCount { get; set; }
    public decimal StartingWeight { get; set; }
    public RepsPerSet ProgramType { get; set; }
}
```

#### Refactored Code

```csharp
public class RepsPerSetStrategy : ExerciseProgression
{
    private RepsPerSetStrategy() { } // For EF Core

    // Properties with private setters
    public RepRange RepRange { get; private set; }
    public int StartingSetCount { get; private set; }
    public int TargetSetCount { get; private set; }
    public int CurrentSetCount { get; private set; } // Tracks current progression
    public decimal StartingWeight { get; private set; }
    public decimal CurrentWeight { get; private set; } // Tracks current weight
    public RepsPerSet ProgramType { get; private set; }

    /// <summary>
    /// Factory method to create Reps Per Set strategy.
    /// </summary>
    public static RepsPerSetStrategy Create(
        Guid exerciseId,
        RepRange repRange,
        int startingSetCount,
        int targetSetCount,
        decimal startingWeight,
        RepsPerSet programType = RepsPerSet.RepsPerSet)
    {
        if (exerciseId == Guid.Empty)
            throw new DomainException("Exercise ID cannot be empty");

        if (startingSetCount < 1)
            throw new DomainException("Starting set count must be at least 1");

        if (targetSetCount < startingSetCount)
            throw new DomainException("Target set count must be >= starting set count");

        if (startingWeight < 0)
            throw new DomainException("Starting weight cannot be negative");

        var strategy = new RepsPerSetStrategy
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            RepRange = repRange,
            StartingSetCount = startingSetCount,
            TargetSetCount = targetSetCount,
            CurrentSetCount = startingSetCount,
            StartingWeight = startingWeight,
            CurrentWeight = startingWeight,
            ProgramType = programType
        };

        strategy.ValidateInvariants();
        return strategy;
    }

    /// <summary>
    /// Applies Reps Per Set progression rules.
    /// Success: Add a set (up to target), then increase weight and reset sets
    /// Maintained/Failed: No change (can only progress, not regress)
    /// </summary>
    public override void ApplyProgression(PerformanceResult result)
    {
        if (result != PerformanceResult.Success)
            return; // Reps Per Set doesn't regress on failure

        // If haven't reached target set count, add a set
        if (CurrentSetCount < TargetSetCount)
        {
            CurrentSetCount++;
        }
        else
        {
            // Reached target sets - increase weight and reset to starting sets
            CurrentWeight += 5m; // Standard 5lb increment
            CurrentSetCount = StartingSetCount;
        }
    }

    /// <summary>
    /// Gets the current number of sets to perform.
    /// </summary>
    public int GetCurrentSets() => CurrentSetCount;

    /// <summary>
    /// Gets the current weight to use.
    /// </summary>
    public decimal GetCurrentWeight() => CurrentWeight;

    protected override void ValidateInvariants()
    {
        if (RepRange == null)
            throw new DomainException("Rep Range is required");

        if (CurrentSetCount > TargetSetCount)
            throw new DomainException("Current set count cannot exceed target");
    }
}
```

---

### Step 4: Update EF Core Configuration

**File**: `HevySync.Infrastructure/Persistence/Configurations/WorkoutConfiguration.cs`

Add configuration for private setters and backing fields:

```csharp
public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("Workouts");

        builder.HasKey(w => w.Id);

        // Configure value object conversions
        builder.Property(w => w.Name)
            .HasConversion(
                name => name.Value,
                value => WorkoutName.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.UserId)
            .HasColumnName("ApplicationUserId")
            .IsRequired();

        // Owned type for WorkoutActivity
        builder.OwnsOne(w => w.Activity, activity =>
        {
            activity.Property(a => a.Week).HasColumnName("Week");
            activity.Property(a => a.Day).HasColumnName("Day");
            activity.Property(a => a.WorkoutsInWeek).HasColumnName("WorkoutsInWeek");
        });

        // Configure backing field for exercises collection
        builder.Metadata
            .FindNavigation(nameof(Workout.Exercises))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Relationship with Exercise
        builder.HasMany<Exercise>("_exercises") // Use backing field
            .WithOne()
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(w => w.DomainEvents);
    }
}
```

**File**: `HevySync.Infrastructure/Persistence/Configurations/ExerciseConfiguration.cs`

```csharp
public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        // Value object conversions
        builder.Property(e => e.Name)
            .HasConversion(
                name => name.Value,
                value => ExerciseName.Create(value))
            .HasColumnName("ExerciseName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RestTimer)
            .HasConversion(
                timer => timer.Seconds,
                seconds => RestTimer.Create(seconds))
            .HasColumnName("RestTimer")
            .IsRequired();

        builder.Property(e => e.ExerciseTemplateId)
            .HasMaxLength(100);

        // Enum properties
        builder.Property(e => e.BodyCategory)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.EquipmentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Relationship with Progression (1:1)
        builder.HasOne(e => e.Progression)
            .WithOne()
            .HasForeignKey<ExerciseProgression>(p => p.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

### Step 5: Update Application Layer

**File**: `HevySync.Application/Workouts/Commands/CreateWorkout/CreateWorkoutCommandHandler.cs`

#### Before

```csharp
public async Task<WorkoutDto> Handle(CreateWorkoutCommand request, CancellationToken cancellationToken)
{
    var workout = new Workout
    {
        Id = Guid.NewGuid(),
        Name = WorkoutName.Create(request.Name),
        UserId = request.UserId,
        Activity = WorkoutActivity.Create(1, 1, request.WorkoutsInWeek),
        Exercises = new List<Exercise>()
    };

    foreach (var exerciseDto in request.Exercises)
    {
        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workout.Id,
            Name = ExerciseName.Create(exerciseDto.Name),
            // ... more properties
        };
        workout.Exercises.Add(exercise);
    }

    await _workoutRepository.AddAsync(workout, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(workout);
}
```

#### After

```csharp
public async Task<WorkoutDto> Handle(CreateWorkoutCommand request, CancellationToken cancellationToken)
{
    // Use factory method
    var workout = Workout.Create(
        name: WorkoutName.Create(request.Name),
        userId: request.UserId,
        workoutsInWeek: request.WorkoutsInWeek
    );

    // Add exercises through aggregate method
    foreach (var exerciseDto in request.Exercises)
    {
        var progression = CreateProgression(exerciseDto);

        workout.AddExercise(
            name: ExerciseName.Create(exerciseDto.Name),
            day: exerciseDto.Day,
            order: exerciseDto.Order,
            numberOfSets: exerciseDto.NumberOfSets,
            restTimer: RestTimer.Create(exerciseDto.RestTimer),
            progression: progression,
            exerciseTemplateId: exerciseDto.ExerciseTemplateId,
            bodyCategory: exerciseDto.BodyCategory,
            equipmentType: exerciseDto.EquipmentType
        );
    }

    await _workoutRepository.AddAsync(workout, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return MapToDto(workout);
}

private ExerciseProgression CreateProgression(ExerciseDto dto)
{
    if (dto.LinearProgressionDto != null)
    {
        return LinearProgressionStrategy.Create(
            exerciseId: Guid.NewGuid(), // Temporary, will be set by EF Core
            trainingMax: TrainingMax.Create(dto.LinearProgressionDto.TrainingMax),
            weightProgression: WeightProgression.Create(dto.LinearProgressionDto.WeightProgression),
            isPrimary: dto.LinearProgressionDto.IsPrimary,
            programType: dto.LinearProgressionDto.ProgramType
        );
    }
    else if (dto.RepsPerSetDto != null)
    {
        return RepsPerSetStrategy.Create(
            exerciseId: Guid.NewGuid(),
            repRange: RepRange.Create(
                dto.RepsPerSetDto.MinimumReps,
                dto.RepsPerSetDto.TargetReps,
                dto.RepsPerSetDto.MaximumTargetReps
            ),
            startingSetCount: dto.RepsPerSetDto.StartingSetCount,
            targetSetCount: dto.RepsPerSetDto.TargetSetCount,
            startingWeight: dto.RepsPerSetDto.StartingWeight
        );
    }

    throw new DomainException("Exercise must have either Linear Progression or Reps Per Set strategy");
}
```

---

**File**: `HevySync.Application/Workouts/Commands/GenerateNextWeek/GenerateNextWeekCommandHandler.cs`

#### Before

```csharp
public async Task<Dictionary<int, List<SessionExerciseDto>>> Handle(...)
{
    var workout = await _workoutRepository.GetByIdWithExercisesAsync(...);

    // Apply progression (stays the same, but now delegates properly)
    workout.ApplyProgression(request.WeekPerformances);

    await _workoutRepository.UpdateAsync(workout, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Generate sets for new week
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
            daySets.Add(new SessionExerciseDto(exercise, sets));
        }

        weekSets[day] = daySets;
    }

    return weekSets;
}
```

#### After

```csharp
public async Task<Dictionary<int, List<SessionExerciseDto>>> Handle(...)
{
    var workout = await _workoutRepository.GetByIdWithExercisesAsync(...);

    // Apply progression (now properly delegated to progression strategies)
    workout.ApplyProgression(request.WeekPerformances);

    await _workoutRepository.UpdateAsync(workout, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // Generate sets using aggregate method
    var exercisesWithSets = workout.GenerateSetsForCurrentWeek(_setGenerationService);

    // Map to DTOs
    var weekSets = exercisesWithSets.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.Select(ews => new SessionExerciseDto(
            ews.Exercise.Id,
            ews.Exercise.Name.Value,
            ews.Sets.ToList(),
            ews.Exercise.RestTimer.Seconds,
            ews.Exercise.ExerciseTemplateId
        )).ToList()
    );

    return weekSets;
}
```

---

### Step 6: Add New Domain Exception

**File**: `HevySync.Domain/Exceptions/DomainException.cs`

Create if it doesn't exist:

```csharp
namespace HevySync.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

---

## Testing Strategy

### Unit Tests to Add/Update

#### Workout Tests

```csharp
[Fact]
public void Create_ValidParameters_ShouldCreateWorkout()
{
    // Arrange
    var name = WorkoutName.Create("A2S Hypertrophy");
    var userId = Guid.NewGuid();

    // Act
    var workout = Workout.Create(name, userId, workoutsInWeek: 5);

    // Assert
    workout.Should().NotBeNull();
    workout.Id.Should().NotBe(Guid.Empty);
    workout.Name.Should().Be(name);
    workout.UserId.Should().Be(userId);
    workout.Activity.Week.Should().Be(1);
    workout.Activity.Day.Should().Be(1);
    workout.Exercises.Should().BeEmpty();
    workout.DomainEvents.Should().ContainSingle(e => e is WorkoutCreatedEvent);
}

[Fact]
public void Create_InvalidWorkoutsInWeek_ShouldThrowDomainException()
{
    // Arrange
    var name = WorkoutName.Create("Test");
    var userId = Guid.NewGuid();

    // Act & Assert
    var act = () => Workout.Create(name, userId, workoutsInWeek: 8);
    act.Should().Throw<DomainException>()
        .WithMessage("Workouts in week must be between 1 and 7");
}

[Fact]
public void AddExercise_ValidExercise_ShouldAddToCollection()
{
    // Arrange
    var workout = CreateValidWorkout();
    var progression = LinearProgressionStrategy.Create(
        Guid.NewGuid(),
        TrainingMax.Create(225),
        WeightProgression.Create(5),
        isPrimary: true
    );

    // Act
    workout.AddExercise(
        ExerciseName.Create("Squat"),
        day: 1,
        order: 1,
        numberOfSets: 4,
        RestTimer.Create(180),
        progression
    );

    // Assert
    workout.Exercises.Should().HaveCount(1);
    workout.Exercises.First().Name.Value.Should().Be("Squat");
}

[Fact]
public void AddExercise_DuplicateDayOrder_ShouldThrowDomainException()
{
    // Arrange
    var workout = CreateValidWorkout();
    AddExerciseToWorkout(workout, day: 1, order: 1);

    // Act & Assert
    var act = () => AddExerciseToWorkout(workout, day: 1, order: 1);
    act.Should().Throw<DomainException>()
        .WithMessage("Exercise already exists at Day 1, Order 1");
}

[Fact]
public void CompleteDay_MissingExercise_ShouldThrowDomainException()
{
    // Arrange
    var workout = CreateWorkoutWithExercises();
    var performances = new List<ExercisePerformance>(); // Missing exercises

    // Act & Assert
    var act = () => workout.CompleteDay(performances);
    act.Should().Throw<DomainException>()
        .WithMessage("Missing performance data for exercises:*");
}
```

#### LinearProgressionStrategy Tests

```csharp
[Fact]
public void ApplyProgression_Success_ShouldIncreaseTrainingMax()
{
    // Arrange
    var strategy = LinearProgressionStrategy.Create(
        Guid.NewGuid(),
        TrainingMax.Create(225),
        WeightProgression.Create(5),
        isPrimary: true
    );

    // Act
    strategy.ApplyProgression(PerformanceResult.Success);

    // Assert
    strategy.TrainingMax.Value.Should().Be(230);
}

[Fact]
public void ApplyProgression_Failed_ShouldDecreaseTrainingMax()
{
    // Arrange
    var strategy = LinearProgressionStrategy.Create(
        Guid.NewGuid(),
        TrainingMax.Create(225),
        WeightProgression.Create(5),
        isPrimary: true
    );

    // Act
    strategy.ApplyProgression(PerformanceResult.Failed);

    // Assert
    strategy.TrainingMax.Value.Should().Be(215); // Decreased by 2x progression
}
```

#### RepsPerSetStrategy Tests

```csharp
[Fact]
public void ApplyProgression_Success_ShouldIncrementSets()
{
    // Arrange
    var strategy = RepsPerSetStrategy.Create(
        Guid.NewGuid(),
        RepRange.Create(8, 10, 12),
        startingSetCount: 3,
        targetSetCount: 5,
        startingWeight: 135
    );

    // Act
    strategy.ApplyProgression(PerformanceResult.Success);

    // Assert
    strategy.CurrentSetCount.Should().Be(4);
    strategy.CurrentWeight.Should().Be(135); // Weight unchanged
}

[Fact]
public void ApplyProgression_SuccessAtTargetSets_ShouldIncreaseWeightAndResetSets()
{
    // Arrange
    var strategy = RepsPerSetStrategy.Create(
        Guid.NewGuid(),
        RepRange.Create(8, 10, 12),
        startingSetCount: 3,
        targetSetCount: 5,
        startingWeight: 135
    );

    // Get to target sets
    strategy.ApplyProgression(PerformanceResult.Success); // 4 sets
    strategy.ApplyProgression(PerformanceResult.Success); // 5 sets

    // Act
    strategy.ApplyProgression(PerformanceResult.Success); // Should increase weight

    // Assert
    strategy.CurrentSetCount.Should().Be(3); // Reset to starting
    strategy.CurrentWeight.Should().Be(140); // Increased by 5lb
}
```

---

## Migration Plan

### Database Migration

After refactoring, you may need to add columns for new properties:

```bash
dotnet ef migrations add AddProgressionTracking --project HevySync.Infrastructure --startup-project HevySync
```

Expected changes:
- Add `CurrentSetCount` to RepsPerSetStrategy
- Add `CurrentWeight` to RepsPerSetStrategy
- Add `ConsecutiveFailures` to LinearProgressionStrategy (if exposed)

---

## Validation Checklist

After completing refactoring, verify:

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Can create workout via API
- [ ] Can generate week 1 sets
- [ ] Can complete workout day
- [ ] Can generate next week with progression
- [ ] EF Core correctly maps private setters
- [ ] Domain events still fire correctly
- [ ] No external code can violate aggregate invariants
- [ ] Application layer uses factory methods
- [ ] Progression logic is delegated to strategies

---

## Common Issues and Solutions

### Issue: EF Core can't set private properties

**Solution**: Use backing fields in configuration:

```csharp
builder.Metadata
    .FindNavigation(nameof(Workout.Exercises))!
    .SetPropertyAccessMode(PropertyAccessMode.Field);
```

### Issue: Domain events not firing

**Solution**: Ensure `AddDomainEvent` is called in factory methods and aggregate methods.

### Issue: Tests failing due to constructor access

**Solution**: Use `InternalsVisibleTo` in `AssemblyInfo.cs` or make constructors `internal` instead of `private` for testing.

```csharp
[assembly: InternalsVisibleTo("HevySync.UnitTests")]
[assembly: InternalsVisibleTo("HevySync.IntegrationTests")]
```

---

## Summary

This refactoring guide provides step-by-step instructions to:

1. ✅ Fix encapsulation by making setters private
2. ✅ Add factory methods for controlled object creation
3. ✅ Protect collections from external manipulation
4. ✅ Add behavior methods to entities (not just data)
5. ✅ Delegate progression logic to strategies
6. ✅ Validate invariants at creation and mutation
7. ✅ Update application layer to use proper patterns
8. ✅ Maintain testability and EF Core compatibility

**Estimated Total Time**: 4-6 hours

**Priority**: Critical - Foundation for all future development

**Next Steps After Refactoring**:
1. Implement Hevy integration
2. Add performance history tracking
3. Build A2S spreadsheet import

