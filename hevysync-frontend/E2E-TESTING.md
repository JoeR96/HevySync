# E2E Testing Guide

## Prerequisites

Before running e2e tests, you need both the backend and frontend running.

### 1. Start the Backend

In a terminal, navigate to the HevySync directory and run:

```bash
cd HevySync
dotnet run
```

The backend will start on `http://localhost:5189`

### 2. Run the E2E Tests

In another terminal, navigate to the frontend directory and run:

```bash
cd hevysync-frontend
npm run test:e2e
```

## Test Commands

- `npm run test:e2e` - Run all tests headless
- `npm run test:e2e:headed` - Run tests with browser visible
- `npm run test:e2e:ui` - Run tests with Playwright UI
- `npm run test:e2e:debug` - Run tests in debug mode

## How Tests Work

The e2e tests:
1. Register a new unique user for each test
2. Create workouts and exercises
3. Test the complete workout flow including progression
4. Each test gets a fresh user to avoid data conflicts

## Notes

- Tests run sequentially (not in parallel) to avoid database conflicts
- Each test creates its own user with a unique email
- The demo user (`demo@hevysync.com`) is NOT used by tests - it's only for manual development
