# HevySync Frontend

React frontend for HevySync workout tracking application.

## Tech Stack

- React 18 with TypeScript
- Vite for build tooling
- Clerk for authentication
- React Router for routing
- Zustand for state management
- Tailwind CSS for styling
- Axios for API requests
- Radix UI for components

## Getting Started

### Prerequisites

- Node.js 18+ and npm
- Backend API running at https://localhost:7109

### Installation

```bash
npm install
```

### Environment Variables

Create a `.env.local` file in the root directory:

```
VITE_CLERK_PUBLISHABLE_KEY=pk_test_d2VsY29tZS1xdWFpbC0wLmNsZXJrLmFjY291bnRzLmRldiQ
VITE_API_BASE_URL=https://localhost:7109
```

### Development

```bash
npm run dev
```

The application will be available at http://localhost:5173

### Demo Login

Use the demo account to explore the application:
- Email: demo@hevysync.com
- Password: Demo123!

## Project Structure

```
src/
├── api/          # API client configuration
├── components/   # React components
│   └── ui/       # Reusable UI components
├── lib/          # Utility functions
├── pages/        # Page components
├── store/        # Zustand stores
├── types/        # TypeScript type definitions
└── hooks/        # Custom React hooks
```

## Features

- Clerk authentication with demo login
- Create and manage workouts
- Support for Linear Progression and Reps Per Set strategies
- Responsive design with Tailwind CSS
- Type-safe API integration

## Build

```bash
npm run build
```

## Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Run ESLint
- `npm run preview` - Preview production build
