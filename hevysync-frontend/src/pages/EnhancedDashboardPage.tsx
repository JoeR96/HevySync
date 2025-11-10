import {useNavigate} from 'react-router-dom';
import {useWorkoutStore} from '../store/workoutStore';
import {useTheme} from '../contexts/ThemeContext';
import {useEffect, useState} from 'react';
import {Activity, Award, Calendar, ChevronRight, Clock, Dumbbell, Moon, Repeat, Sun, Weight} from 'lucide-react';
import {DashboardCard, StatCard} from '../components/DashboardCard';
import {ExerciseProgressChart} from '../components/ExerciseProgressChart';
import {BodyPartsChart} from '../components/BodyPartsChart';
import {OneRepMaxChart} from '../components/OneRepMaxChart';
import {CompleteWorkoutModal} from '../components/CompleteWorkoutModal';
import WeeklyExercisesList from '../components/WeeklyExercisesList';

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
    const {theme, toggleTheme} = useTheme();
    const {workouts, fetchWorkouts, isLoading, completeDay, fetchWeekSessions} = useWorkoutStore();
    const [userEmail, setUserEmail] = useState<string>('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [weekSessions, setWeekSessions] = useState<any>(null);

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

    useEffect(() => {
        const token = localStorage.getItem('authToken');
        if (!token) {
            navigate('/login');
        } else {
            setUserEmail('demo@hevysync.com');
            fetchWorkouts();
        }
    }, [navigate, fetchWorkouts]);

    useEffect(() => {
        if (activeWorkout) {
            const workoutId = getProp(activeWorkout, 'id');
            fetchWeekSessions(workoutId).then(sessions => {
                console.log('Week sessions:', sessions);
                setWeekSessions(sessions);
            }).catch(err => console.error('Failed to load week sessions:', err));
        }
    }, [activeWorkout, fetchWeekSessions]);

    const handleSignOut = () => {
        localStorage.removeItem('authToken');
        navigate('/login');
    };

    const handleCompleteWorkout = async (performances: any[]) => {
        if (!activeWorkout) return;
        const workoutId = getProp(activeWorkout, 'id');
        await completeDay(workoutId, performances);
        await fetchWorkouts();
    };

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
            acc.push({name: bodyPart, value: 1});
        }
        return acc;
    }, []);

    // Sample data for tracked exercise
    const squatProgressData = [
        {date: 'Oct 14', weight: 125},
        {date: 'Oct 21', weight: 127.5},
        {date: 'Oct 28', weight: 130},
        {date: 'Nov 4', weight: 130},
        {date: 'Nov 11', weight: 132.5},
        {date: 'Nov 18', weight: 135},
        {date: 'Nov 25', weight: 137.5},
        {date: 'Dec 2', weight: 140},
    ];

    // Sample 1RM data - in real app, this would come from latest workout performances
    const oneRepMaxData = [
        {exerciseName: 'Squat', weight: 140, reps: 5},
        {exerciseName: 'Bench Press', weight: 100, reps: 8},
        {exerciseName: 'Deadlift', weight: 160, reps: 3},
        {exerciseName: 'Overhead Press', weight: 60, reps: 6},
        {exerciseName: 'Barbell Row', weight: 80, reps: 10},
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
                            <Dumbbell className="h-8 w-8 text-indigo-600 dark:text-indigo-400 mr-3"/>
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
                                    <Sun className="h-5 w-5 text-gray-600 dark:text-gray-300"/>
                                ) : (
                                    <Moon className="h-5 w-5 text-gray-600"/>
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
                        icon={<Activity className="h-8 w-8 text-indigo-600 dark:text-indigo-400"/>}
                    />
                    <StatCard
                        title="Total Workouts"
                        value={workouts.length}
                        subtitle={`${completedWorkouts.length} completed`}
                        icon={<Calendar className="h-8 w-8 text-indigo-600 dark:text-indigo-400"/>}
                    />
                    <StatCard
                        title="Total Exercises"
                        value={totalExercises}
                        subtitle="Across all workouts"
                        icon={<Dumbbell className="h-8 w-8 text-indigo-600 dark:text-indigo-400"/>}
                    />
                    <StatCard
                        title="Weeks Trained"
                        value={totalWeeksCompleted}
                        subtitle="Total completed"
                        icon={<Award className="h-8 w-8 text-indigo-600 dark:text-indigo-400"/>}
                    />
                </div>

                {/* Mosaic Dashboard Layout */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
                    <ExerciseProgressChart
                        exerciseName="Squat"
                        trainingMax={140}
                        data={squatProgressData}
                    />
                    <OneRepMaxChart data={oneRepMaxData as any}/>
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
                                    onClick={() => setIsModalOpen(true)}
                                    className="px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-lg hover:from-indigo-700 hover:to-purple-700 transition-all shadow-md flex items-center font-semibold"
                                >
                                    Complete Workout
                                    <ChevronRight className="ml-2 h-5 w-5"/>
                                </button>
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                {todaysExercises.map((exercise: any) => {
                                    const exerciseId = getProp(exercise, 'id');
                                    const exerciseName = getProp(exercise, 'exerciseName');
                                    const numberOfSets = getProp(exercise, 'numberOfSets');
                                    const restTimer = getProp(exercise, 'restTimer');
                                    const exerciseDetail = getProp(exercise, 'exerciseDetail');

                                    // Get sets from week sessions for this exercise's day
                                    const daySessions = weekSessions?.[currentDay] || [];
                                    const exerciseSession = daySessions.find((s: any) =>
                                        (getProp(s, 'exerciseName') || '').toLowerCase() === (exerciseName || '').toLowerCase()
                                    );
                                    const sets = getProp(exerciseSession, 'sets') || [];
                                    const workingWeight = sets.length > 0 ? getProp(sets[0], 'weightKg') : null;
                                    const targetReps = sets.length > 0 ? getProp(sets[0], 'reps') : null;
                                    const amrapReps = sets.length > 1 ? getProp(sets[sets.length - 1], 'reps') : targetReps;

                                    // Get progression details for display
                                    const trainingMax = getProp(exerciseDetail, 'trainingMax');
                                    const startingWeight = getProp(exerciseDetail, 'startingWeight');
                                    const minReps = getProp(exerciseDetail, 'minimumReps');
                                    const maxReps = getProp(exerciseDetail, 'maximumTargetReps');
                                    const program = getProp(exerciseDetail, 'program') || getProp(exerciseDetail, 'programType');

                                    // Check if it's A2S Hypertrophy (linear progression)
                                    const isA2SHypertrophy = program === 'Average2SavageHypertrophy' || program === 'LinearProgression' || trainingMax;

                                    return (
                                        <div key={exerciseId}
                                             className="border-2 border-indigo-200 dark:border-indigo-800 rounded-lg p-4 hover:shadow-lg transition-all bg-white dark:bg-gray-800">
                                            <div className="flex items-start justify-between mb-3">
                                                <div className="flex-1">
                                                    <h4 className="font-bold text-gray-900 dark:text-white text-base">{exerciseName}</h4>
                                                    <div className="flex items-center gap-2 mt-1">
                                                        <p className="text-xs text-gray-600 dark:text-gray-400 font-semibold">{getBodyPart(exerciseName)}</p>
                                                        <span className={`text-xs px-2 py-0.5 rounded-full font-bold ${
                                                            isA2SHypertrophy
                                                                ? 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-400'
                                                                : 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400'
                                                        }`}>
                                                            {isA2SHypertrophy ? 'Linear Progression' : 'RepsPerSet'}
                                                        </span>
                                                    </div>
                                                </div>
                                                <div
                                                    className="bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-400 px-3 py-1 rounded-full text-xs font-bold">
                                                    {numberOfSets} sets
                                                </div>
                                            </div>

                                            <div className="space-y-2 mt-3">
                                                {/* Training Max or Working Weight */}
                                                {isA2SHypertrophy && trainingMax ? (
                                                    <div
                                                        className="flex items-center justify-between text-sm bg-purple-50 dark:bg-purple-900/20 p-2 rounded">
                                                        <span className="text-gray-700 dark:text-gray-300 flex items-center font-bold text-xs">
                                                            Training Max:
                                                        </span>
                                                        <span className="font-bold text-purple-700 dark:text-purple-400 text-base">{trainingMax}kg</span>
                                                    </div>
                                                ) : workingWeight && (
                                                    <div
                                                        className="flex items-center justify-between text-sm bg-blue-50 dark:bg-blue-900/20 p-2 rounded">
                                                        <span className="text-gray-700 dark:text-gray-300 flex items-center font-bold text-xs">
                                                            Working Weight:
                                                        </span>
                                                        <span className="font-bold text-blue-700 dark:text-blue-400 text-base">{workingWeight}kg</span>
                                                    </div>
                                                )}

                                                {/* Sets breakdown */}
                                                {sets.length > 0 && (
                                                    <div className="bg-gray-50 dark:bg-gray-900/50 p-3 rounded">
                                                        <div className="space-y-1">
                                                            {sets.map((set: any, idx: number) => {
                                                                const setWeight = getProp(set, 'weightKg');
                                                                const setReps = getProp(set, 'reps');
                                                                const isLastSet = idx === sets.length - 1 && isA2SHypertrophy;

                                                                return (
                                                                    <div key={idx} className="flex items-center justify-between text-xs">
                                                                        <span className="text-gray-600 dark:text-gray-400 font-semibold">
                                                                            Set {idx + 1}:
                                                                        </span>
                                                                        <span className="font-bold text-gray-900 dark:text-white">
                                                                            {setWeight}kg × {setReps}{isLastSet ? '+' : ''} reps
                                                                            {isLastSet && <span className="ml-1 text-purple-600 dark:text-purple-400">(AMRAP)</span>}
                                                                        </span>
                                                                    </div>
                                                                );
                                                            })}
                                                        </div>
                                                    </div>
                                                )}

                                                <div
                                                    className="flex items-center justify-between text-sm pt-2 border-t border-gray-200 dark:border-gray-700">
                                                    <span className="text-gray-600 dark:text-gray-400 flex items-center font-bold text-xs">
                                                        <Clock className="h-3 w-3 mr-1"/>
                                                        Rest Timer:
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
                    </div>
                )}

                {/* Weekly Exercises List */}
                {activeWorkout && (
                    <div className="mb-6">
                        <WeeklyExercisesList />
                    </div>
                )}

                {/* All Workouts and Body Parts */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
                    <div className="lg:col-span-2">
                        <DashboardCard>
                            <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
                                <Calendar className="h-6 w-6 mr-2 text-indigo-600 dark:text-indigo-400"/>
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
                                                <div
                                                    className="flex items-center mt-2 space-x-4 text-sm text-gray-600 dark:text-gray-400 font-semibold">
                      <span className="flex items-center">
                        <Calendar className="h-4 w-4 mr-1"/>
                        Week {week}, Day {day}
                      </span>
                                                    <span className="flex items-center">
                        <Dumbbell className="h-4 w-4 mr-1"/>
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
                                                <ChevronRight className="h-5 w-5 text-gray-400"/>
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        </DashboardCard>
                    </div>

                    {/* Body Parts Distribution */}
                    <BodyPartsChart data={bodyPartsData}/>
                </div>
            </main>

            {/* Complete Workout Modal */}
            {activeWorkout && (
                <CompleteWorkoutModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    exercises={todaysExercises}
                    onComplete={handleCompleteWorkout}
                    workoutName={getProp(activeWorkout, 'name') || ''}
                    week={currentWeek}
                    day={currentDay}
                    weekSessions={weekSessions}
                />
            )}
        </div>
    );
}
