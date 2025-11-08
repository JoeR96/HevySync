import { useUser, SignOutButton } from '@clerk/clerk-react';
import { useNavigate } from 'react-router-dom';
import { useWorkoutStore } from '../store/workoutStore';
import { useEffect } from 'react';

export default function DashboardPage() {
  const { user, isLoaded } = useUser();
  const navigate = useNavigate();
  const { currentWorkout, workouts } = useWorkoutStore();

  useEffect(() => {
    if (isLoaded && !user) {
      navigate('/login');
    }
  }, [isLoaded, user, navigate]);

  if (!isLoaded || !user) {
    return <div>Loading...</div>;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold">HevySync</h1>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-700">
                {user.emailAddresses[0]?.emailAddress}
              </span>
              <SignOutButton>
                <button className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700">
                  Sign Out
                </button>
              </SignOutButton>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-2xl font-bold text-gray-900">Dashboard</h2>
            <button
              onClick={() => navigate('/create-workout')}
              className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
            >
              Create New Workout
            </button>
          </div>

          {currentWorkout ? (
            <div className="bg-white shadow rounded-lg p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">
                Current Workout: {currentWorkout.Name}
              </h3>
              <div className="space-y-2">
                <p className="text-sm text-gray-600">
                  Week: {currentWorkout.WorkoutActivity.Week} | Day: {currentWorkout.WorkoutActivity.Day}
                </p>
                <p className="text-sm text-gray-600">
                  Exercises: {currentWorkout.Exercises.length}
                </p>
              </div>
            </div>
          ) : (
            <div className="bg-white shadow rounded-lg p-6">
              <p className="text-gray-500">No active workout. Create one to get started!</p>
            </div>
          )}

          {workouts.length > 0 && (
            <div className="bg-white shadow rounded-lg p-6">
              <h3 className="text-lg font-medium text-gray-900 mb-4">All Workouts</h3>
              <div className="space-y-4">
                {workouts.map((workout) => (
                  <div key={workout.Id} className="border-l-4 border-indigo-500 pl-4">
                    <h4 className="font-medium text-gray-900">{workout.Name}</h4>
                    <p className="text-sm text-gray-600">
                      {workout.Exercises.length} exercises
                    </p>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
