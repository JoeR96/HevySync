import {useState} from 'react';
import {useNavigate} from 'react-router-dom';
import {useWorkoutStore} from '../store/workoutStore';
import {type CreateExerciseRequest, ExerciseProgram} from "../types/exercise.ts";

export default function CreateWorkoutPage() {
    const navigate = useNavigate();
    const {createWorkout, isLoading} = useWorkoutStore();
    const [workoutName, setWorkoutName] = useState('');
    const [workoutDays, setWorkoutDays] = useState(5);
    const [exercises, setExercises] = useState<CreateExerciseRequest[]>([]);

    const handleAddExercise = () => {
        setExercises([
            ...exercises,
            {
                ExerciseName: '',
                ExerciseTemplateId: '',
                RestTimer: 120,
                Day: 1,
                Order: exercises.length,
                ExerciseDetailsRequest: {
                    Program: ExerciseProgram.Average2SavageRepsPerSet,
                    MinimumReps: 8,
                    TargetReps: 10,
                    MaximumTargetReps: 12,
                    NumberOfSets: 3,
                    TotalNumberOfSets: 5,
                    StartingWeight: 0,
                    WeightProgression: 2.5,
                },
            },
        ]);
    };

    const handleRemoveExercise = (index: number) => {
        setExercises(exercises.filter((_, i) => i !== index));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!workoutName || exercises.length === 0) {
            alert('Please provide a workout name and at least one exercise');
            return;
        }

        try {
            await createWorkout({
                WorkoutName: workoutName,
                WorkoutDaysInWeek: workoutDays,
                Exercises: exercises,
            });
            navigate('/dashboard');
        } catch (error) {
            console.error('Failed to create workout:', error);
            alert('Failed to create workout. Please try again.');
        }
    };

    return (
        <div className="min-h-screen bg-gray-50">
            <nav className="bg-white shadow">
                <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                    <div className="flex justify-between h-16">
                        <div className="flex items-center">
                            <h1 className="text-xl font-bold">HevySync</h1>
                        </div>
                        <button
                            onClick={() => navigate('/dashboard')}
                            className="text-sm text-gray-600 hover:text-gray-900"
                        >
                            Back to Dashboard
                        </button>
                    </div>
                </div>
            </nav>

            <main className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
                <h2 className="text-2xl font-bold text-gray-900 mb-8">Create New Workout</h2>

                <form onSubmit={handleSubmit} className="space-y-6">
                    <div className="bg-white shadow rounded-lg p-6 space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Workout Name
                            </label>
                            <input
                                type="text"
                                value={workoutName}
                                onChange={(e) => setWorkoutName(e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                placeholder="e.g., A2S Hypertrophy Program"
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Days per Week
                            </label>
                            <input
                                type="number"
                                min="1"
                                max="7"
                                value={workoutDays}
                                onChange={(e) => setWorkoutDays(parseInt(e.target.value))}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-indigo-500"
                                required
                            />
                        </div>
                    </div>

                    <div className="space-y-4">
                        <div className="flex justify-between items-center">
                            <h3 className="text-lg font-medium text-gray-900">Exercises</h3>
                            <button
                                type="button"
                                onClick={handleAddExercise}
                                className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700"
                            >
                                Add Exercise
                            </button>
                        </div>

                        {exercises.map((exercise, index) => (
                            <div key={index} className="bg-white shadow rounded-lg p-6">
                                <div className="flex justify-between items-start mb-4">
                                    <h4 className="text-md font-medium text-gray-900">Exercise {index + 1}</h4>
                                    <button
                                        type="button"
                                        onClick={() => handleRemoveExercise(index)}
                                        className="text-red-600 hover:text-red-700"
                                    >
                                        Remove
                                    </button>
                                </div>
                                <div className="grid grid-cols-2 gap-4">
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Exercise Name
                                        </label>
                                        <input
                                            type="text"
                                            value={exercise.ExerciseName}
                                            onChange={(e) => {
                                                const newExercises = [...exercises];
                                                newExercises[index].ExerciseName = e.target.value;
                                                setExercises(newExercises);
                                            }}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Template ID
                                        </label>
                                        <input
                                            type="text"
                                            value={exercise.ExerciseTemplateId}
                                            onChange={(e) => {
                                                const newExercises = [...exercises];
                                                newExercises[index].ExerciseTemplateId = e.target.value;
                                                setExercises(newExercises);
                                            }}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Day
                                        </label>
                                        <input
                                            type="number"
                                            min="1"
                                            max={workoutDays}
                                            value={exercise.Day}
                                            onChange={(e) => {
                                                const newExercises = [...exercises];
                                                newExercises[index].Day = parseInt(e.target.value);
                                                setExercises(newExercises);
                                            }}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                            required
                                        />
                                    </div>
                                    <div>
                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                            Rest Timer (seconds)
                                        </label>
                                        <input
                                            type="number"
                                            min="0"
                                            value={exercise.RestTimer}
                                            onChange={(e) => {
                                                const newExercises = [...exercises];
                                                newExercises[index].RestTimer = parseInt(e.target.value);
                                                setExercises(newExercises);
                                            }}
                                            className="w-full px-3 py-2 border border-gray-300 rounded-md"
                                            required
                                        />
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>

                    <div className="flex justify-end space-x-4">
                        <button
                            type="button"
                            onClick={() => navigate('/dashboard')}
                            className="px-6 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={isLoading}
                            className="px-6 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50"
                        >
                            {isLoading ? 'Creating...' : 'Create Workout'}
                        </button>
                    </div>
                </form>
            </main>
        </div>
    );
}
