# HevySync Architecture Documentation

**Project**: A2S (Average to Savage) Spreadsheet Migration to Web App
**Last Updated**: 2025-12-25

---

## Document Index

This directory contains comprehensive DDD architecture documentation for the HevySync application.

### 📋 [ARCHITECTURE_REVIEW.md](./ARCHITECTURE_REVIEW.md)

**Complete architectural analysis from a DDD perspective**

Contains:
- Bounded context analysis
- Aggregate design review with diagrams
- Current system flows (4 detailed flows)
- Missing components identification
- DDD violations and improvements
- Encapsulation issues with code examples
- Recommended changes prioritized by impact

**Key Findings**:
- ✅ Good use of value objects, domain events, and CQRS separation
- 🔴 Critical encapsulation violations (public setters, exposed collections)
- 🔴 Anemic domain model (entities lack behavior)
- ❌ Hevy integration missing (core feature)
- ❌ Performance history not persisted
- ❌ A2S spreadsheet import not implemented

**Read this first** to understand the overall architecture and what needs improvement.

---

### 🔧 [REFACTORING_GUIDE.md](./REFACTORING_GUIDE.md)

**Step-by-step guide to fix encapsulation and improve DDD design**

Contains:
- Detailed refactoring checklist (4 phases)
- Complete code examples for:
  - Workout aggregate with factory method and protected collections
  - Exercise entity with behavior methods
  - ExerciseProgression with polymorphic ApplyProgression()
  - EF Core configuration for private setters
  - Application layer updates
- Testing strategy with example unit tests
- Migration plan for database changes
- Common issues and solutions

**Estimated Effort**: 4-6 hours

**Priority**: Critical - Must be done before adding new features

**Use this** when implementing the encapsulation fixes.

---

### 🔗 [HEVY_INTEGRATION_PLAN.md](./HEVY_INTEGRATION_PLAN.md)

**Complete plan for integrating with Hevy mobile app**

Contains:
- User flow from HevySync to Hevy app
- Hevy API understanding (assumed endpoints)
- Integration architecture with bounded context design
- Implementation plan (5 phases):
  1. Domain model changes (2 hours)
  2. API client implementation (4 hours)
  3. Exercise mapping service (3 hours)
  4. Event-driven sync (3 hours)
  5. API endpoints for mapping (2-3 hours)
- Complete code examples for:
  - HevyApiClient with retry logic
  - ExerciseMapping aggregate
  - WorkoutCreatedEventHandler for auto-sync
  - HevyWorkoutMapper (anti-corruption layer)
- Error handling and retry strategies
- Open questions about Hevy API

**Estimated Effort**: 12-16 hours

**Priority**: Critical (Core Product Feature)

**Use this** when implementing Hevy integration.

---

## Quick Start

### If you're new to the codebase:

1. Read **ARCHITECTURE_REVIEW.md** sections:
   - Bounded Contexts
   - Aggregate Design Review
   - Current System Flows

2. Review the current flows to understand how the system works:
   - Flow 1: Create A2S Workout Program
   - Flow 2: Generate Week 1 Sets (A2S Calculations)
   - Flow 3: Complete Workout Day
   - Flow 4: Generate Next Week (Apply A2S Progression)

### If you're implementing improvements:

1. **First priority**: Follow **REFACTORING_GUIDE.md**
   - Fix encapsulation violations
   - Add factory methods
   - Protect collections
   - Move behavior to entities

2. **Second priority**: Implement **HEVY_INTEGRATION_PLAN.md**
   - Add Hevy API client
   - Create exercise mapping service
   - Wire up event handlers
   - Test end-to-end sync

3. **Third priority**: Add missing features (from ARCHITECTURE_REVIEW.md)
   - Performance history tracking
   - A2S spreadsheet import
   - Deload weeks
   - User preferences

---

## Architecture Summary

### Current State

```
┌─────────────────────────────────────────────────────────────────┐
│                    WORKOUT MANAGEMENT CONTEXT                    │
│                                                                  │
│  Aggregate: Workout                                              │
│  ├─ Entities: Exercise, ExerciseProgression                      │
│  ├─ Value Objects: WorkoutName, RestTimer, Set, TrainingMax     │
│  ├─ Domain Events: WorkoutCreated, WorkoutDayCompleted          │
│  └─ Domain Services: SetGenerationService, Calculators          │
│                                                                  │
│  Current Features:                                               │
│  ✅ Create A2S workout programs                                  │
│  ✅ Generate sets using A2S formulas (server-side)               │
│  ✅ Track workout completion                                     │
│  ✅ Apply progressive overload                                   │
│                                                                  │
│  Missing Features:                                               │
│  ❌ Sync to Hevy app                                             │
│  ❌ Performance history                                          │
│  ❌ A2S spreadsheet import                                       │
│  ❌ Deload automation                                            │
│                                                                  │
│  Architecture Issues:                                            │
│  🔴 Broken encapsulation (public setters)                        │
│  🔴 Anemic entities (no behavior)                                │
│  🔴 Progression logic in wrong layer                             │
│  🟡 Missing bounded context separation                           │
└─────────────────────────────────────────────────────────────────┘
```

### Target State (After Improvements)

```
┌──────────────────────────────────────────────────────────────────┐
│                        BOUNDED CONTEXTS                           │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌────────────────────┐       ┌────────────────────┐            │
│  │ PROGRAM DESIGN     │       │ HEVY INTEGRATION   │            │
│  │                    │       │                    │            │
│  │ - Workout          │───────│ - HevyApiClient    │            │
│  │ - Exercise         │ OHS   │ - ExerciseMapping  │            │
│  │ - Progression      │       │ - WorkoutSync      │            │
│  │ - Calculators      │       │                    │            │
│  └────────────────────┘       └────────────────────┘            │
│           │                                                       │
│           │                                                       │
│           ▼                                                       │
│  ┌────────────────────┐       ┌────────────────────┐            │
│  │ TRAINING SESSION   │       │ USER MANAGEMENT    │            │
│  │                    │       │                    │            │
│  │ - Performance      │       │ - User             │            │
│  │ - History          │       │ - Preferences      │            │
│  │ - Progress         │       │                    │            │
│  └────────────────────┘       └────────────────────┘            │
│                                                                   │
└──────────────────────────────────────────────────────────────────┘
```

---

## DDD Patterns Applied

### ✅ Currently Implemented

- **Aggregate Pattern**: Workout as aggregate root with Exercise children
- **Value Objects**: Immutable objects for domain concepts (WorkoutName, TrainingMax, etc.)
- **Domain Events**: WorkoutCreatedEvent, WorkoutDayCompletedEvent
- **Repository Pattern**: IWorkoutRepository abstracts persistence
- **Domain Services**: Stateless services for complex calculations
- **CQRS-like Pattern**: Separate command and query handlers

### 🔴 Needs Improvement

- **Encapsulation**: Properties should have private setters, collections should be read-only
- **Factory Methods**: Aggregates should use factories to ensure valid creation
- **Behavior Methods**: Entities should have methods, not just properties
- **Bounded Contexts**: Consider splitting as system grows
- **Anti-Corruption Layer**: Needed for Hevy API integration

### 🆕 To Be Added

- **Event Handlers**: For cross-aggregate coordination (Hevy sync)
- **Specification Pattern**: For complex querying (performance history)
- **Strategy Pattern Refinement**: Progression strategies should update themselves
- **Domain Services**: Additional services for Hevy mapping, import parsing

---

## Key Metrics

### Code Quality

| Metric | Current | Target |
|--------|---------|--------|
| Public setters on aggregates | 🔴 Yes | ✅ None |
| Exposed collections | 🔴 Yes | ✅ Protected |
| Anemic entities | 🔴 High | ✅ Low |
| Factory methods | 🟡 Partial | ✅ All aggregates |
| Domain logic in application layer | 🟡 Some | ✅ Minimal |
| Test coverage | 🟡 ~60% | ✅ >80% |

### Features

| Feature | Status | Priority |
|---------|--------|----------|
| Create A2S workouts | ✅ Complete | - |
| Generate sets (A2S formulas) | ✅ Complete | - |
| Track workout completion | ✅ Complete | - |
| Apply progression | ✅ Complete | - |
| Sync to Hevy app | ❌ Not started | 🔴 Critical |
| Performance history | ❌ Not started | 🟡 High |
| A2S import | ❌ Not started | 🟡 Medium |
| Deload automation | 🟡 Partial | 🟢 Low |

---

## Recommended Reading Order

### For Developers Fixing Bugs

1. ARCHITECTURE_REVIEW.md → Current System Flows
2. Code in `/HevySync.Domain` and `/HevySync.Application`
3. REFACTORING_GUIDE.md → Common Issues section

### For Developers Adding Features

1. ARCHITECTURE_REVIEW.md → Full read
2. REFACTORING_GUIDE.md → Understand proper patterns
3. HEVY_INTEGRATION_PLAN.md (if working on integration)
4. Implement changes following the guides

### For Architects/Tech Leads

1. ARCHITECTURE_REVIEW.md → Full read
2. REFACTORING_GUIDE.md → Validate approach
3. HEVY_INTEGRATION_PLAN.md → Review architecture
4. Assess bounded context split timing

---

## Contributing

When making changes to the architecture:

1. Update relevant documentation in this folder
2. Add diagrams for new flows or bounded contexts
3. Document new aggregates, entities, and value objects
4. Update this README if new docs are added
5. Keep code examples in sync with actual implementation

---

## Questions or Issues?

If you have questions about the architecture or need clarification:

1. Check the relevant document first (ARCHITECTURE_REVIEW, REFACTORING_GUIDE, HEVY_INTEGRATION_PLAN)
2. Review the "Common Issues and Solutions" sections
3. Look at the code examples provided
4. Open a GitHub issue with tag `architecture` or `ddd`

---

**Remember**: Good architecture is about protecting invariants, making the implicit explicit, and aligning code with the business domain. These documents are living documentation - keep them updated as the system evolves.
