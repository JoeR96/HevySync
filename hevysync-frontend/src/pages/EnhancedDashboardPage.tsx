import { useNavigate } from 'react-router-dom';
import { useWorkoutStore } from '../store/workoutStore';
import { useTheme } from '../contexts/ThemeContext';
import { useEffect, useState } from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import { Calendar, Dumbbell, Clock, Award, Activity, ChevronRight, Moon, Sun, Weight, Repeat, Target } from 'lucide-react';

// Temporary body part mapping - will be replaced with backend data
const EXERCISE_BODY_PARTS: Record<string, string> = {
  'squat': 'Legs',
  'deadlift': 'Back',
  'bench': 'Chest',
  'press': 'Shoulders',
  'row': 'Back',
  'curl': 'Arms',
  'pullup': 'Back',
  'pulldown': 'Back',
  'leg': 'Legs',
  'calf': 'Legs',
  'rdl': 'Legs',
  'romanian': 'Legs',
  'tricep': 'Arms',
  'lateral': 'Shoulders',
  'fly': 'Chest',
  'incline': 'Chest',
  'overhead': 'Shoulders',
  'ohp': 'Shoulders',
  'face': 'Shoulders',
  'bicep': 'Arms',
  'bulgarian': 'Legs',
  'extension': 'Legs',
  'ab': 'Core',
  'plank': 'Core',
};

function getBodyPart(exerciseName: string): string {
  const name = exerciseName.toLowerCase();
  for (const [key, value] of Object.entries(EXERCISE_BODY_PARTS)) {
    if (name.includes(key)) return value;
  }
  return 'Other';
}

export default function EnhancedDashboardPage() {
  const navigate = useNavigate();
  const { theme, toggleTheme } = useTheme();
  const { workouts, fetchWorkouts, isLoading } = useWorkoutStore();
  const [userEmail, setUserEmail] = useState<string>('');

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      navigate('/login');
    } else {
      setUserEmail('demo@hevysync.com');
      fetchWorkouts();
    }
  }, [navigate, fetchWorkouts]);

  const handleSignOut = () => {
    localStorage.removeItem('authToken');
    navigate('/login');
  };

  const getProp = (obj: any, propName: string) => {
    if (!obj) return undefined;
    const lowerProp = propName.toLowerCase();
    const key = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
    return key ? obj[key] : undefined;
  };

  const completedWorkouts = workouts.filter((w: any) => {
    const activity = getProp(w, 'workoutActivity');
    const status = getProp(activity, 'status');
    return status === 'Completed';
  });

  const activeWorkout = workouts.find((w: any) => {
    const activity = getProp(w, 'workoutActivity');
    const status = getProp(activity, 'status');
    return status === 'Active';
  }) as any;

  const totalExercises = workouts.reduce((sum: number, w: any) => {
    const exercises = getProp(w, 'exercises');
    return sum + (exercises?.length || 0);
  }, 0);

  const totalWeeksCompleted = completedWorkouts.reduce((sum: number, w: any) => {
    const activity = getProp(w, 'workoutActivity');
    const week = getProp(activity, 'week');
    return sum + (week || 0);
  }, 0);

  const activeActivity = activeWorkout ? getProp(activeWorkout, 'workoutActivity') : null;
  const currentDay = activeActivity ? (getProp(activeActivity, 'day') || 1) : 1;
  const currentWeek = activeActivity ? (getProp(activeActivity, 'week') || 1) : 1;
  const allExercises = activeWorkout ? (getProp(activeWorkout, 'exercises') || []) : [];
  const todaysExercises = allExercises.filter((ex: any) => getProp(ex, 'day') === currentDay);

  // Calculate body parts distribution
  const bodyPartsData = allExercises.reduce((acc: any[], ex: any) => {
    const exerciseName = getProp(ex, 'exerciseName') || '';
    const bodyPart = getBodyPart(exerciseName);
    const existing = acc.find(item => item.name === bodyPart);
    if (existing) {
      existing.value += 1;
    } else {
      acc.push({ name: bodyPart, value: 1 });
    }
    return acc;
  }, []);

  const COLORS = ['#ff9900', '#c27100', '#ea8600', '#9a5a00', '#7c4a00'];

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-dark-bg">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-brand-orange mx-auto"></div>
          <p className="mt-4 text-dark-text">Loading your workouts...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-dark-bg transition-colors">
      {/* Header */}
      <nav className="bg-dark-surface shadow-md border-b border-dark-border">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Dumbbell className="h-8 w-8 text-brand-orange mr-3" />
              <h1 className="text-2xl font-bold text-brand-orange">
                HevySync
              </h1>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={toggleTheme}
                className="p-2 rounded-lg hover:bg-dark-elevated transition-colors"
                aria-label="Toggle theme"
              >
                {theme === 'dark' ? (
                  <Sun className="h-5 w-5 text-dark-text" />
                ) : (
                  <Moon className="h-5 w-5 text-gray-600" />
                )}
              </button>
              <span className="text-sm font-medium text-dark-text">{userEmail}</span>
              <button
                onClick={handleSignOut}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-all shadow-sm"
              >
                Sign Out
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Hero Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-dark-surface border border-dark-border rounded-2xl p-6 shadow-lg hover:shadow-xl transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-dark-text text-sm font-medium">Active Workout</p>
                <p className="text-3xl font-bold mt-2 text-white">{activeWorkout ? 'In Progress' : 'None'}</p>
                {activeWorkout && activeActivity && (
                  <p className="text-dark-text text-sm mt-1">Week {currentWeek}, Day {currentDay}</p>
                )}
              </div>
              <div className="bg-brand-orange/20 p-3 rounded-full">
                <Activity className="h-8 w-8 text-brand-orange" />
              </div>
            </div>
          </div>

          <div className="bg-dark-surface border border-dark-border rounded-2xl p-6 shadow-lg hover:shadow-xl transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-dark-text text-sm font-medium">Total Workouts</p>
                <p className="text-3xl font-bold mt-2 text-white">{workouts.length}</p>
                <p className="text-dark-text text-sm mt-1">{completedWorkouts.length} completed</p>
              </div>
              <div className="bg-brand-orange/20 p-3 rounded-full">
                <Calendar className="h-8 w-8 text-brand-orange" />
              </div>
            </div>
          </div>

          <div className="bg-dark-surface border border-dark-border rounded-2xl p-6 shadow-lg hover:shadow-xl transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-dark-text text-sm font-medium">Total Exercises</p>
                <p className="text-3xl font-bold mt-2 text-white">{totalExercises}</p>
                <p className="text-dark-text text-sm mt-1">Across all workouts</p>
              </div>
              <div className="bg-brand-orange/20 p-3 rounded-full">
                <Dumbbell className="h-8 w-8 text-brand-orange" />
              </div>
            </div>
          </div>

          <div className="bg-dark-surface border border-dark-border rounded-2xl p-6 shadow-lg hover:shadow-xl transition-shadow">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-dark-text text-sm font-medium">Weeks Trained</p>
                <p className="text-3xl font-bold mt-2 text-white">{totalWeeksCompleted}</p>
                <p className="text-dark-text text-sm mt-1">Total completed</p>
              </div>
              <div className="bg-brand-orange/20 p-3 rounded-full">
                <Award className="h-8 w-8 text-brand-orange" />
              </div>
            </div>
          </div>
        </div>

        {/* Mosaic Dashboard Layout */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          {/* Tracked Exercise Widget - Placeholder */}
          <div className="bg-dark-surface rounded-2xl p-6 shadow-lg border border-dark-border">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-bold text-white flex items-center">
                <Target className="h-5 w-5 mr-2 text-brand-orange" />
                Tracked Exercise
              </h3>
            </div>
            <div className="text-center py-8">
              <p className="text-dark-text">Select an exercise to track its progression</p>
              <button className="mt-4 px-4 py-2 bg-brand-orange text-white rounded-lg hover:bg-orange-600 transition-colors">
                Track Exercise
              </button>
            </div>
          </div>

          {/* Body Parts Distribution Pie Chart */}
          <div className="bg-dark-surface rounded-2xl p-6 shadow-lg border border-dark-border">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-bold text-white flex items-center">
                <Target className="h-5 w-5 mr-2 text-brand-orange" />
                Body Parts Distribution
              </h3>
            </div>
            {bodyPartsData.length > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={bodyPartsData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, value }) => `${name}: ${value}`}
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="value"
                  >
                    {bodyPartsData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip 
                    contentStyle={{ 
                      backgroundColor: '#313131', 
                      border: '1px solid #454545', 
                      borderRadius: '8px',
                      color: '#fff'
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <div className="text-center py-8 text-dark-text">
                No exercises to display
              </div>
            )}
          </div>
        </div>

        {/* Current Workout Detail - Compact */}
        {activeWorkout && todaysExercises.length > 0 && (
          <div className="bg-dark-surface rounded-2xl p-6 shadow-lg mb-8 border border-dark-border">
            <div className="flex items-center justify-between mb-6">
              <div>
                <h3 className="text-2xl font-bold text-white">{getProp(activeWorkout, 'name')}</h3>
                <p className="text-dark-text mt-1">
                  Week {currentWeek} • Day {currentDay} • {todaysExercises.length} exercises
                </p>
              </div>
              <button
                onClick={() => navigate('/workout/execute')}
                className="px-6 py-3 bg-brand-orange text-white rounded-xl hover:bg-orange-600 transition-all shadow-md flex items-center"
              >
                Start Workout
                <ChevronRight className="ml-2 h-5 w-5" />
              </button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {todaysExercises.map((exercise: any) => {
                const exerciseId = getProp(exercise, 'id');
                const exerciseName = getProp(exercise, 'exerciseName');
                const numberOfSets = getProp(exercise, 'numberOfSets');
                const restTimer = getProp(exercise, 'restTimer');
                const exerciseDetail = getProp(exercise, 'exerciseDetail');
                
                // Get progression details
                const trainingMax = getProp(exerciseDetail, 'trainingMax');
                const startingWeight = getProp(exerciseDetail, 'startingWeight');
                const targetReps = getProp(exerciseDetail, 'targetReps');
                const minReps = getProp(exerciseDetail, 'minimumReps');
                const maxReps = getProp(exerciseDetail, 'maximumTargetReps');
                const program = getProp(exerciseDetail, 'program');
                
                // Check if it's A2S Hypertrophy (linear progression)
                const isA2SHypertrophy = program === 'Average2SavageHypertrophy' || trainingMax;
                
                return (
                  <div key={exerciseId} className="border border-dark-border rounded-xl p-4 hover:shadow-md transition-shadow bg-dark-elevated">
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex-1">
                        <h4 className="font-semibold text-white">{exerciseName}</h4>
                        <p className="text-xs text-dark-text mt-1">{getBodyPart(exerciseName)}</p>
                      </div>
                      <div className="bg-brand-orange/20 text-brand-orange px-3 py-1 rounded-full text-xs font-medium">
                        {numberOfSets} sets
                      </div>
                    </div>
                    
                    <div className="space-y-2 mt-3">
                      {isA2SHypertrophy && trainingMax && (
                        <>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-dark-text flex items-center">
                              <Weight className="h-3 w-3 mr-1" />
                              Training Max
                            </span>
                            <span className="font-semibold text-white">{trainingMax} kg</span>
                          </div>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-dark-text flex items-center">
                              <Target className="h-3 w-3 mr-1" />
                              AMRAP Target
                            </span>
                            <span className="font-semibold text-brand-orange">Week {currentWeek}</span>
                          </div>
                        </>
                      )}
                      {startingWeight && (
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-dark-text flex items-center">
                            <Weight className="h-3 w-3 mr-1" />
                            Weight
                          </span>
                          <span className="font-semibold text-white">{startingWeight} kg</span>
                        </div>
                      )}
                      {(targetReps || minReps) && (
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-dark-text flex items-center">
                            <Repeat className="h-3 w-3 mr-1" />
                            Reps
                          </span>
                          <span className="font-semibold text-white">
                            {minReps && maxReps ? `${minReps}-${maxReps}` : targetReps}
                          </span>
                        </div>
                      )}
                      <div className="flex items-center justify-between text-sm pt-2 border-t border-dark-border">
                        <span className="text-dark-text flex items-center">
                          <Clock className="h-3 w-3 mr-1" />
                          Rest
                        </span>
                        <span className="font-semibold text-white">{restTimer}s</span>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}

        {/* All Workouts List */}
        <div className="bg-dark-surface rounded-2xl p-6 shadow-lg border border-dark-border">
          <h3 className="text-xl font-bold text-white mb-4 flex items-center">
            <Calendar className="h-6 w-6 mr-2 text-brand-orange" />
            All Workouts
          </h3>
          <div className="space-y-3">
            {workouts.map((workout: any) => {
              const workoutId = getProp(workout, 'id');
              const workoutName = getProp(workout, 'name');
              const workoutActivity = getProp(workout, 'workoutActivity');
              const workoutExercises = getProp(workout, 'exercises') || [];
              const week = getProp(workoutActivity, 'week');
              const day = getProp(workoutActivity, 'day');
              const status = getProp(workoutActivity, 'status');
              
              return (
                <div
                  key={workoutId}
                  className="flex items-center justify-between p-4 border border-dark-border rounded-xl hover:border-brand-orange hover:shadow-md transition-all cursor-pointer bg-dark-elevated"
                  onClick={() => navigate(`/workout/${workoutId}`)}
                >
                  <div className="flex-1">
                    <h4 className="font-semibold text-white">{workoutName}</h4>
                    <div className="flex items-center mt-2 space-x-4 text-sm text-dark-text">
                      <span className="flex items-center">
                        <Calendar className="h-4 w-4 mr-1" />
                        Week {week}, Day {day}
                      </span>
                      <span className="flex items-center">
                        <Dumbbell className="h-4 w-4 mr-1" />
                        {workoutExercises.length} exercises
                      </span>
                    </div>
                  </div>
                  <div className="flex items-center space-x-3">
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                      status === 'Active'
                        ? 'bg-green-900/30 text-green-400 border border-green-700'
                        : 'bg-dark-elevated text-dark-text border border-dark-border'
                    }`}>
                      {status || 'Pending'}
                    </span>
                    <ChevronRight className="h-5 w-5 text-dark-text" />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </main>
    </div>
  );
}
