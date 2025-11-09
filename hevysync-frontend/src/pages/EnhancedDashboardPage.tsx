import { useNavigate } from 'react-router-dom';
import { useWorkoutStore } from '../store/workoutStore';
import { useTheme } from '../contexts/ThemeContext';
import { useEffect, useState } from 'react';
import { Calendar, Dumbbell, Clock, Award, Activity, ChevronRight, Moon, Sun, Weight, Repeat, Target } from 'lucide-react';
import { StatCard, DashboardCard } from '../components/DashboardCard';
import { ExerciseProgressChart } from '../components/ExerciseProgressChart';
import { BodyPartsChart } from '../components/BodyPartsChart';
import { OneRepMaxChart } from '../components/OneRepMaxChart';

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

  // Sample data for tracked exercise
  const squatProgressData = [
    { date: 'Oct 14', weight: 125 },
    { date: 'Oct 21', weight: 127.5 },
    { date: 'Oct 28', weight: 130 },
    { date: 'Nov 4', weight: 130 },
    { date: 'Nov 11', weight: 132.5 },
    { date: 'Nov 18', weight: 135 },
    { date: 'Nov 25', weight: 137.5 },
    { date: 'Dec 2', weight: 140 },
  ];

  // Sample 1RM data - in real app, this would come from latest workout performances
  const oneRepMaxData = [
    { exerciseName: 'Squat', weight: 140, reps: 5 },
    { exerciseName: 'Bench Press', weight: 100, reps: 8 },
    { exerciseName: 'Deadlift', weight: 160, reps: 3 },
    { exerciseName: 'Overhead Press', weight: 60, reps: 6 },
    { exerciseName: 'Barbell Row', weight: 80, reps: 10 },
  ];

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
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 transition-colors">
      {/* Header */}
      <nav className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Dumbbell className="h-8 w-8 text-indigo-600 dark:text-indigo-400 mr-3" />
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
                HevySync
              </h1>
            </div>
            <div className="flex items-center space-x-4">
              <button
                onClick={toggleTheme}
                className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                aria-label="Toggle theme"
              >
                {theme === 'dark' ? (
                  <Sun className="h-5 w-5 text-gray-600 dark:text-gray-300" />
                ) : (
                  <Moon className="h-5 w-5 text-gray-600" />
                )}
              </button>
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{userEmail}</span>
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

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        {/* Hero Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <StatCard
            title="Active Workout"
            value={activeWorkout ? 'In Progress' : 'None'}
            subtitle={activeWorkout && activeActivity ? `Week ${currentWeek}, Day ${currentDay}` : undefined}
            icon={<Activity className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />}
          />
          <StatCard
            title="Total Workouts"
            value={workouts.length}
            subtitle={`${completedWorkouts.length} completed`}
            icon={<Calendar className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />}
          />
          <StatCard
            title="Total Exercises"
            value={totalExercises}
            subtitle="Across all workouts"
            icon={<Dumbbell className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />}
          />
          <StatCard
            title="Weeks Trained"
            value={totalWeeksCompleted}
            subtitle="Total completed"
            icon={<Award className="h-8 w-8 text-indigo-600 dark:text-indigo-400" />}
          />
        </div>

        {/* Mosaic Dashboard Layout */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
          <ExerciseProgressChart
            exerciseName="Squat"
            trainingMax={140}
            data={squatProgressData}
          />
          <OneRepMaxChart data={oneRepMaxData as any} />
        </div>

        {/* Current Workout */}
        {activeWorkout && todaysExercises.length > 0 && (
          <div className="space-y-4 mb-6">
            <DashboardCard>
              <div className="flex items-center justify-between mb-6">
                <div>
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white">{getProp(activeWorkout, 'name')}</h3>
                  <p className="text-gray-600 dark:text-gray-400 mt-1 font-semibold">
                    Week {currentWeek} • Day {currentDay} • {todaysExercises.length} exercises
                  </p>
                </div>
                <button
                  onClick={() => navigate('/workout/execute')}
                  className="px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-lg hover:from-indigo-700 hover:to-purple-700 transition-all shadow-md flex items-center font-semibold"
                >
                  Complete Workout
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
                
                // Calculate AMRAP target based on week (A2S Hypertrophy pattern)
                const amrapTargets = [10, 8, 6, 9, 7, 5]; // Repeating pattern
                const amrapTarget = amrapTargets[(currentWeek - 1) % 6];
                
                return (
                  <div key={exerciseId} className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:shadow-md transition-shadow bg-white dark:bg-gray-800">
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex-1">
                        <h4 className="font-bold text-gray-900 dark:text-white">{exerciseName}</h4>
                        <p className="text-xs text-gray-600 dark:text-gray-400 font-semibold mt-1">{getBodyPart(exerciseName)}</p>
                      </div>
                      <div className="bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 px-3 py-1 rounded-full text-xs font-bold">
                        {numberOfSets} sets
                      </div>
                    </div>
                    
                    <div className="space-y-2 mt-3">
                      {isA2SHypertrophy && trainingMax && (
                        <>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold">
                              <Weight className="h-3 w-3 mr-1" />
                              Training Max
                            </span>
                            <span className="font-bold text-gray-900 dark:text-white">{trainingMax} kg</span>
                          </div>
                          <div className="flex items-center justify-between text-sm">
                            <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold">
                              <Target className="h-3 w-3 mr-1" />
                              AMRAP Target
                            </span>
                            <span className="font-bold text-indigo-600 dark:text-indigo-400">{amrapTarget}+ reps</span>
                          </div>
                        </>
                      )}
                      {startingWeight && (
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold">
                            <Weight className="h-3 w-3 mr-1" />
                            Weight
                          </span>
                          <span className="font-bold text-gray-900 dark:text-white">{startingWeight} kg</span>
                        </div>
                      )}
                      {(targetReps || minReps) && (
                        <div className="flex items-center justify-between text-sm">
                          <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold">
                            <Repeat className="h-3 w-3 mr-1" />
                            Reps
                          </span>
                          <span className="font-bold text-gray-900 dark:text-white">
                            {minReps && maxReps ? `${minReps}-${maxReps}` : targetReps}
                          </span>
                        </div>
                      )}
                      <div className="flex items-center justify-between text-sm pt-2 border-t border-gray-200 dark:border-gray-700">
                        <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold">
                          <Clock className="h-3 w-3 mr-1" />
                          Rest
                        </span>
                        <span className="font-bold text-gray-900 dark:text-white">{restTimer}s</span>
                      </div>
                    </div>
                  </div>
                );
              })}
              </div>
            </DashboardCard>

            {/* Week Overview - Horizontal */}
            <DashboardCard>
              <div className="flex items-center mb-3">
                <Calendar className="h-5 w-5 mr-2 text-indigo-600 dark:text-indigo-400" />
                <h3 className="text-lg font-bold text-gray-900 dark:text-white">
                  This Week
                </h3>
              </div>
              <div className="grid grid-cols-5 gap-2">
                {[1, 2, 3, 4, 5].map((day) => {
                  const dayExercises = allExercises.filter((ex: any) => getProp(ex, 'day') === day);
                  const isCurrentDay = day === currentDay;
                  
                  return (
                    <div 
                      key={day} 
                      className={`p-2 rounded border transition-all ${
                        isCurrentDay 
                          ? 'bg-indigo-50 dark:bg-indigo-900/20 border-indigo-500' 
                          : 'bg-gray-50 dark:bg-gray-700/50 border-gray-200 dark:border-gray-600'
                      }`}
                    >
                      <div className="text-center mb-1">
                        <span className={`font-bold text-xs ${isCurrentDay ? 'text-indigo-600 dark:text-indigo-400' : 'text-gray-900 dark:text-white'}`}>
                          Day {day}
                        </span>
                      </div>
                      <div className="text-center text-xs text-gray-600 dark:text-gray-400 mb-1 font-semibold">
                        {dayExercises.length} exercises
                      </div>
                      {dayExercises.length > 0 && (
                        <div className="space-y-0.5">
                          {dayExercises.map((ex: any, idx: number) => {
                            const exerciseName = getProp(ex, 'exerciseName') || '';
                            return (
                              <div 
                                key={idx}
                                className="text-xs text-gray-700 dark:text-gray-300 truncate text-center font-medium"
                                title={exerciseName}
                              >
                                • {exerciseName}
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </DashboardCard>
          </div>
        )}

        {/* All Workouts and Body Parts */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <div className="lg:col-span-2">
            <DashboardCard>
              <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
                <Calendar className="h-6 w-6 mr-2 text-indigo-600 dark:text-indigo-400" />
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
                  className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-lg hover:border-indigo-500 hover:shadow-md transition-all cursor-pointer bg-gray-50 dark:bg-gray-800"
                  onClick={() => navigate(`/workout/${workoutId}`)}
                >
                  <div className="flex-1">
                    <h4 className="font-bold text-gray-900 dark:text-white">{workoutName}</h4>
                    <div className="flex items-center mt-2 space-x-4 text-sm text-gray-600 dark:text-gray-400 font-semibold">
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
                    <span className={`px-3 py-1 rounded-full text-sm font-semibold ${
                      status === 'Active'
                        ? 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400'
                        : 'bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300'
                    }`}>
                      {status || 'Pending'}
                    </span>
                    <ChevronRight className="h-5 w-5 text-gray-400" />
                  </div>
                </div>
              );
            })}
              </div>
            </DashboardCard>
          </div>

          {/* Body Parts Distribution */}
          <BodyPartsChart data={bodyPartsData} />
        </div>
      </main>
    </div>
  );
}
