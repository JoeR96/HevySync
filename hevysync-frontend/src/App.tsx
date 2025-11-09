import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider } from './contexts/ThemeContext';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import EnhancedDashboardPage from './pages/EnhancedDashboardPage';
import CreateWorkoutPage from './pages/CreateWorkoutPage';
import WorkoutExecutionPage from './pages/WorkoutExecutionPage';
import AuthCallbackPage from './pages/AuthCallbackPage';

function App() {
  return (
    <ThemeProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/auth/callback" element={<AuthCallbackPage />} />
          <Route path="/dashboard" element={<EnhancedDashboardPage />} />
          <Route path="/create-workout" element={<CreateWorkoutPage />} />
          <Route path="/workout/execute" element={<WorkoutExecutionPage />} />
          <Route path="/" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
