# HevySync Backend Refactoring Summary

## Completed Tasks ✅

1. **WeightProgression added to RepsPerSetStrategy**
   - Updated domain entity
   - Updated EF configuration
   - Updated command handlers
   - Updated DTOs

2. **BodyCategory and EquipmentType removed**
   - Removed from Exercise domain entity
   - Removed from EF configuration
   - Removed from DTOs
   - Removed from command handlers

3. **Docker Compose created**
   - `docker-compose.local.yml` for PostgreSQL database

4. **Activity Aggregate created**
   - New aggregate with domain events
   - ActivityStartedEvent, ActivityCompletedEvent, ActivityStoppedEvent
   - Single active activity constraint enforced
   - Repository and EF configuration added

5. **Generic Repository and Unit of Work pattern implemented**
   - IRepository<TEntity, TKey>
   - IUnitOfWork with Workouts and Activities repositories
   - Infrastructure implementations complete

6. **MediatR integration started**
   - MediatR package installed
   - Application DependencyInjection updated
   - CreateWorkoutCommand migrated to IRequest
   - CreateWorkoutCommandHandler migrated to IRequestHandler
   - GetWorkoutQuery migrated to IRequest
   - GetWorkoutQueryHandler migrated to IRequestHandler

## Remaining Tasks 📋

### Phase 1: Complete MediatR Migration

Need to migrate remaining commands/queries:

1. **GenerateWeekOneCommand** → IRequest<Dictionary<int, List<SessionExerciseDto>>>
2. **CompleteWorkoutDayCommand** → IRequest<CompleteWorkoutDayResult>
3. **GenerateNextWeekCommand** → IRequest<Dictionary<int, List<SessionExerciseDto>>>

For each:
- Update command/query to inherit from IRequest<TResult>
- Update handler to implement IRequestHandler<TCommand, TResult>
- Change HandleAsync → Handle
- Use IUnitOfWork instead of concrete repositories

### Phase 2: Remove PerformanceResult from Request DTOs

**Current State**: ExercisePerformanceRequest has PerformanceResult property

**Required Changes**:
1. Remove `PerformanceResult` from `ExercisePerformanceRequest` in:
   - `HevySync/Endpoints/Average2Savage/Requests/CompleteWorkoutDayRequest.cs`
   - `HevySync/Endpoints/Average2Savage/Requests/GenerateNextWeekRequest.cs`

2. Implement evaluation logic in progression strategies:
   - Add method `PerformanceResult EvaluatePerformance(List<Set> prescribedSets, List<Set> completedSets)` to:
     - `LinearProgressionStrategy`
     - `RepsPerSetStrategy`

3. Update handlers to call evaluation:
   - `CompleteWorkoutDayCommandHandler`
   - `GenerateNextWeekCommandHandler`

### Phase 3: Clean Up Old Interfaces

Delete:
- `HevySync.Application/Common/ICommand.cs`
- `HevySync.Application/Common/ICommandHandler.cs`
- `HevySync.Application/Common/IQuery.cs`
- `HevySync.Application/Common/IQueryHandler.cs`
- `HevySync.Domain/Repositories/IWorkoutRepository.cs`
- `HevySync.Domain/Repositories/IActivityRepository.cs`
- `HevySync.Infrastructure/Persistence/Repositories/WorkoutRepository.cs`
- `HevySync.Infrastructure/Persistence/Repositories/ActivityRepository.cs`

Remove all `using HevySync.Application.Common` statements

### Phase 4: Update Endpoints to Use MediatR

Update `HevySync/Endpoints/Average2Savage/Average2SavageHandler.cs`:

Change from:
```csharp
private readonly ICommandHandler<CreateWorkoutCommand, WorkoutDto> _createWorkoutHandler;
```

To:
```csharp
private readonly IMediator _mediator;
```

Then use:
```csharp
var result = await _mediator.Send(new CreateWorkoutCommand(...), cancellationToken);
```

### Phase 5: Update Request DTOs

Remove `BodyCategory` and `EquipmentType` from:
- `HevySync/Endpoints/Average2Savage/Requests/LinearProgressionExerciseDetailsRequest.cs`
- `HevySync/Endpoints/Average2Savage/Responses/LinearProgressionDto.cs`
- Any validators that reference these properties

### Phase 6: Create Database Migration

```bash
dotnet ef migrations add RemoveBodyCategoryEquipmentTypeAddActivityAndWeightProgression \
  --project HevySync.Infrastructure \
  --startup-project HevySync \
  --context HevySyncDbContext
```

Then:
```bash
dotnet ef database update --project HevySync.Infrastructure --startup-project HevySync
```

### Phase 7: Create Database Seeder

Create `HevySync.Infrastructure/Persistence/DatabaseSeeder.cs`:

Seed data requirements:
- **User**: "demo@hevysync.com" / "Demo123!"
- **Completed Workout**: A full 21-week program that was completed
- **Current Active Workout**: 21-week, 5-day split
  - Mix of A2S Hypertrophy (Linear Progression) and Reps Per Set
  - Current position: Week 11, Day 3
  - 5-6 exercises per day
  - Logical exercise selection (Squat, Bench, Deadlift, OHP, accessories)

Exercises distribution:
- Day 1: Lower (Squat primary + accessories)
- Day 2: Upper (Bench primary + accessories)
- Day 3: Lower (Deadlift primary + accessories)
- Day 4: Upper (OHP primary + accessories)
- Day 5: Full body/accessories

## Frontend Tasks (Next Phase)

After backend is complete:

1. Create React project with Vite
2. Install dependencies from package.json
3. Set up Clerk authentication
4. Integrate with .NET Identity
5. Build workout creation flow
6. Build workout execution flow
7. Add "Demo Login" button with pre-seeded user

## Migration Script

Create PowerShell script to automate remaining MediatR migration:

```powershell
# migrate-to-mediatr.ps1
$commands = @(
    "GenerateWeekOneCommand",
    "CompleteWorkoutDayCommand",
    "GenerateNextWeekCommand"
)

foreach ($cmd in $commands) {
    # Update command file
    # Update handler file
    # Details in script...
}
```

## Build & Run

Once complete:

```bash
# Start database
docker compose -f docker-compose.local.yml up -d

# Run migrations
dotnet ef database update --project HevySync.Infrastructure --startup-project HevySync

# Run application
dotnet run --project HevySync
```

## Notes

- All primary constructors should be used where applicable
- Arrow functions for single-expression methods
- Sad paths before happy paths
- Remove all inline comments (keep XML summaries)
- No redundant namespaces

## Current Build Status

Last known issues:
- Need to complete MediatR migration for remaining 3 commands
- Need to remove old ICommand/IQuery interfaces
- Need to update endpoints to use IMediator

Estimated time to complete backend: 2-3 hours
