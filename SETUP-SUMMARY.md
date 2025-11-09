# HevySync Setup Summary

## What's Been Configured

### Backend (ASP.NET Core)
- ✅ Running on `http://localhost:5189`
- ✅ Swagger UI available at `http://localhost:5189/swagger/index.html`
- ✅ Database seeder configured with demo user
- ✅ Demo user credentials:
  - Email: `demo@hevysync.com`
  - Password: `Chicken1234!`

### Demo User Seeded Data
The demo user has:
- **2 workouts** pre-seeded:
  1. **A2S Hypertrophy - Completed Cycle**: 21 weeks completed with 5 exercises per day
  2. **A2S Hypertrophy - Current Cycle**: 10 weeks + 2 days completed (currently active)

Each workout includes:
- 5 days per week
- 5-6 exercises per day
- Realistic progression data
- Mix of linear progression and reps-per-set strategies

### Frontend (React + Vite)
- ✅ Running on `http://localhost:5173`
- ✅ Registration page at `/register`
- ✅ Login page at `/login` with demo login button
- ✅ Dashboard at `/dashboard`
- ✅ API client configured to use Bearer token authentication
- ✅ Dashboard handles both camelCase and PascalCase API responses

### E2E Tests (Playwright)
- ✅ Tests configured to register new users (not use demo user)
- ✅ Global setup waits for backend to be ready
- ✅ Tests run sequentially to avoid database conflicts
- ✅ Each test creates a unique user with timestamp-based email

## How to Use

### Manual Testing with Demo User
1. Navigate to `http://localhost:5173/login`
2. Click "Demo Login (Pre-loaded Data)" button
3. You'll be redirected to the dashboard with 2 workouts and realistic data

### Running E2E Tests
1. Ensure backend is running: `dotnet run --project HevySync/HevySync.csproj`
2. Run tests: `cd hevysync-frontend && npm run test:e2e`

See `hevysync-frontend/E2E-TESTING.md` for more details.

## API Endpoints

### Authentication
- `POST /register` - Register new user
- `POST /login?useCookies=false` - Login (returns JWT token)

### Workouts
- `GET /average2savage/workouts` - Get all user workouts
- `POST /average2savage/workout` - Create new workout
- `POST /average2savage/workout/complete-day` - Complete a workout day
- `POST /average2savage/workout/generate-next-week` - Generate next week

## Fixed Issues

1. ✅ Database seeder password updated to match test expectations
2. ✅ Fixed WeightProgression validation (Ab Wheel and Plank exercises had 0 progression)
3. ✅ Added registration page for e2e tests
4. ✅ Updated authentication to use Bearer tokens instead of cookies
5. ✅ Fixed dashboard to handle both camelCase and PascalCase API responses
6. ✅ Added helper function `getProp()` for case-insensitive property access

## Current Status

Both frontend and backend are running and connected. The demo user can login and see their seeded workout data on the dashboard.
