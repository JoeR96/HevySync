import {useNavigate} from 'react-router-dom';
import {useWorkoutStore} from '../store/workoutStore';
import {useEffect, useState} from 'react';
import {
    CartesianGrid,
    Cell,
    Line,
    LineChart,
    Pie,
    PieChart,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis
} from 'recharts';
import {Activity, Award, Calendar, ChevronRight, Clock, Dumbbell, Target, TrendingUp} from 'lucide-react';
import CurrentCycleSessions from '../components/CurrentCycleSessions';

export default function DashboardPage() {
    const navigate = useNavigate();
    const {currentWorkout, workouts, fetchWorkouts, isLoading} = useWorkoutStore();
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

    const COLORS = ['#6366f1', '#8b5cf6', '#ec4899', '#f59e0b'];

    // Helper function to get property value (handles both camelCase and PascalCase)
    const getProp = (obj: any, propName: string) => {
        if (!obj) return undefined;
        const lowerProp = propName.toLowerCase();
        const key = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
        return key ? obj[key] : undefined;
    };

    // Calculate stats from workouts
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

    // Weekly progress data
    const activeActivity = activeWorkout ? getProp(activeWorkout, 'workoutActivity') : null;
    const currentWeek = activeActivity ? (getProp(activeActivity, 'week') || 0) : 0;
    const weeklyData = activeWorkout ? Array.from({length: Math.min(currentWeek, 10)}, (_, i) => ({
        week: `W${i + 1}`,
        progress: 85 + Math.random() * 15
    })) : [];

    // Exercise distribution
    const exercises = activeWorkout ? (getProp(activeWorkout, 'exercises') || []) : [];
    const exercisesByDay = exercises.reduce((acc: any[], ex: any) => {
        const day = getProp(ex, 'day');
        const existing = acc.find(item => item.name === `Day ${day}`);
        if (existing) {
            existing.count += 1;
        } else {
            acc.push({name: `Day ${day}`, count: 1});
        }
        return acc;
    }, []);

    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-indigo-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Loading your workouts...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
            {/* Header */}
            <nav className="bg-white shadow-md border-b border-gray-200">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between h-16">
                        <div className="flex items-center">
                            <Dumbbell className="h-8 w-8 text-indigo-600 mr-3"/>
                            <h1 className="text-2xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                                HevySync
                            </h1>
                        </div>
                        <div className="flex items-center space-x-4">
                            <span className="text-sm font-medium text-gray-700">{userEmail}</span>
                            <button
                                onClick={handleSignOut}
                                className="px-4 py-2 text-sm font-medium text-white bg-gradient-to-r from-red-500 to-red-600 rounded-lg hover:from-red-600 hover:to-red-700 transition-all shadow-sm"
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
                    <div
                        className="bg-gradient-to-br from-indigo-500 to-indigo-600 rounded-2xl p-6 text-white shadow-lg hover:shadow-xl transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-indigo-100 text-sm font-medium">Active Workout</p>
                                <p className="text-3xl font-bold mt-2">{activeWorkout ? 'In Progress' : 'None'}</p>
                                {activeWorkout && activeActivity && (
                                    <p className="text-indigo-100 text-sm mt-1">Week {getProp(activeActivity, 'week')},
                                        Day {getProp(activeActivity, 'day')}</p>
                                )}
                            </div>
                            <div className="bg-white/20 p-3 rounded-full">
                                <Activity className="h-8 w-8"/>
                            </div>
                        </div>
                    </div>

                    <div
                        className="bg-gradient-to-br from-purple-500 to-purple-600 rounded-2xl p-6 text-white shadow-lg hover:shadow-xl transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-purple-100 text-sm font-medium">Total Workouts</p>
                                <p className="text-3xl font-bold mt-2">{workouts.length}</p>
                                <p className="text-purple-100 text-sm mt-1">{completedWorkouts.length} completed</p>
                            </div>
                            <div className="bg-white/20 p-3 rounded-full">
                                <Calendar className="h-8 w-8"/>
                            </div>
                        </div>
                    </div>

                    <div
                        className="bg-gradient-to-br from-pink-500 to-pink-600 rounded-2xl p-6 text-white shadow-lg hover:shadow-xl transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-pink-100 text-sm font-medium">Total Exercises</p>
                                <p className="text-3xl font-bold mt-2">{totalExercises}</p>
                                <p className="text-pink-100 text-sm mt-1">Across all workouts</p>
                            </div>
                            <div className="bg-white/20 p-3 rounded-full">
                                <Dumbbell className="h-8 w-8"/>
                            </div>
                        </div>
                    </div>

                    <div
                        className="bg-gradient-to-br from-orange-500 to-orange-600 rounded-2xl p-6 text-white shadow-lg hover:shadow-xl transition-shadow">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-orange-100 text-sm font-medium">Weeks Trained</p>
                                <p className="text-3xl font-bold mt-2">{totalWeeksCompleted}</p>
                                <p className="text-orange-100 text-sm mt-1">Total completed</p>
                            </div>
                            <div className="bg-white/20 p-3 rounded-full">
                                <Award className="h-8 w-8"/>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Charts Row */}
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
                    {/* Weekly Progress Chart */}
                    <div className="bg-white rounded-2xl p-6 shadow-lg">
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-bold text-gray-900 flex items-center">
                                <TrendingUp className="h-5 w-5 mr-2 text-indigo-600"/>
                                Weekly Progress
                            </h3>
                        </div>
                        <ResponsiveContainer width="100%" height={250}>
                            <LineChart data={weeklyData}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0"/>
                                <XAxis dataKey="week" stroke="#6b7280"/>
                                <YAxis stroke="#6b7280"/>
                                <Tooltip
                                    contentStyle={{
                                        backgroundColor: '#fff',
                                        border: '1px solid #e5e7eb',
                                        borderRadius: '8px'
                                    }}
                                />
                                <Line
                                    type="monotone"
                                    dataKey="progress"
                                    stroke="#6366f1"
                                    strokeWidth={3}
                                    dot={{fill: '#6366f1', r: 4}}
                                />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>

                    {/* Exercise Distribution */}
                    <div className="bg-white rounded-2xl p-6 shadow-lg">
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-bold text-gray-900 flex items-center">
                                <Target className="h-5 w-5 mr-2 text-purple-600"/>
                                Exercise Distribution
                            </h3>
                        </div>
                        <ResponsiveContainer width="100%" height={250}>
                            <PieChart>
                                <Pie
                                    data={exercisesByDay}
                                    cx="50%"
                                    cy="50%"
                                    labelLine={false}
                                    label={({name, count}) => `${name}: ${count}`}
                                    outerRadius={80}
                                    fill="#8884d8"
                                    dataKey="count"
                                >
                                    {exercisesByDay.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]}/>
                                    ))}
                                </Pie>
                                <Tooltip/>
                            </PieChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Current Workout Detail */}
                {activeWorkout && activeActivity && (
                    <div className="bg-white rounded-2xl p-6 shadow-lg mb-8">
                        <div className="flex items-center justify-between mb-6">
                            <div>
                                <h3 className="text-2xl font-bold text-gray-900">{getProp(activeWorkout, 'name')}</h3>
                                <p className="text-gray-600 mt-1">
                                    Week {getProp(activeActivity, 'week')} of {getProp(activeActivity, 'workoutsInWeek')} •
                                    Day {getProp(activeActivity, 'day')}
                                </p>
                            </div>
                            <button
                                onClick={() => navigate('/workout/execute')}
                                className="px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-xl hover:from-indigo-700 hover:to-purple-700 transition-all shadow-md flex items-center"
                            >
                                Start Today's Workout
                                <ChevronRight className="ml-2 h-5 w-5"/>
                            </button>
                        </div>

                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                            {exercises.slice(0, 6).map((exercise: any) => {
                                const exerciseId = getProp(exercise, 'id');
                                const exerciseName = getProp(exercise, 'exerciseName');
                                const day = getProp(exercise, 'day');
                                const numberOfSets = getProp(exercise, 'numberOfSets');
                                const restTimer = getProp(exercise, 'restTimer');
                                const plannedSets = getProp(exercise, 'plannedSets') || [];

                                return (
                                    <div key={exerciseId}
                                         className="border border-gray-200 rounded-xl p-4 hover:shadow-md transition-shadow">
                                        <div className="flex items-start justify-between mb-2">
                                            <div className="flex-1">
                                                <h4 className="font-semibold text-gray-900">{exerciseName}</h4>
                                                <p className="text-sm text-gray-500 mt-1">Day {day}</p>
                                            </div>
                                            <div
                                                className="bg-indigo-50 text-indigo-700 px-3 py-1 rounded-full text-xs font-medium">
                                                {numberOfSets} sets
                                            </div>
                                        </div>

                                        {plannedSets.length > 0 && (
                                            <div className="mt-3 mb-2">
                                                <div className="flex flex-wrap gap-1">
                                                    {plannedSets.map((set: any, idx: number) => {
                                                        const weight = getProp(set, 'weightKg');
                                                        const reps = getProp(set, 'reps');
                                                        return (
                                                            <div
                                                                key={idx}
                                                                className="px-2 py-1 bg-gray-100 text-gray-700 rounded text-xs font-medium"
                                                            >
                                                                {weight}kg × {reps}
                                                            </div>
                                                        );
                                                    })}
                                                </div>
                                            </div>
                                        )}

                                        <div className="mt-3 flex items-center text-sm text-gray-600">
                                            <Clock className="h-4 w-4 mr-1"/>
                                            {restTimer}s rest
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                )}

                {/* Current Cycle Sessions */}
                {activeWorkout && (
                    <CurrentCycleSessions/>
                )}

                {/* All Workouts List */}
                <div className="bg-white rounded-2xl p-6 shadow-lg">
                    <h3 className="text-xl font-bold text-gray-900 mb-4 flex items-center">
                        <Calendar className="h-6 w-6 mr-2 text-indigo-600"/>
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
                                    className="flex items-center justify-between p-4 border border-gray-200 rounded-xl hover:border-indigo-300 hover:shadow-md transition-all cursor-pointer"
                                >
                                    <div className="flex-1">
                                        <h4 className="font-semibold text-gray-900">{workoutName}</h4>
                                        <div className="flex items-center mt-2 space-x-4 text-sm text-gray-600">
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
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                        status === 'Active'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                    }`}>
                      {status || 'Pending'}
                    </span>
                                        <ChevronRight className="h-5 w-5 text-gray-400"/>
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
