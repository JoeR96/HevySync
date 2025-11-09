import { useState, useEffect } from 'react';
import { X, Plus, Trash2, CheckCircle, XCircle, Send } from 'lucide-react';

interface CompletedSet {
    WeightKg: number;
    Reps: number;
}

interface ExercisePerformance {
    exerciseId: string;
    exerciseName: string;
    sets: CompletedSet[];
    performanceResult: 'Success' | 'Failed';
}

interface CompleteWorkoutModalProps {
    isOpen: boolean;
    onClose: () => void;
    exercises: any[];
    onComplete: (performances: any[]) => Promise<void>;
    workoutName: string;
    week: number;
    day: number;
    weekSessions?: any;
}

export function CompleteWorkoutModal({
    isOpen,
    onClose,
    exercises,
    onComplete,
    workoutName,
    week,
    day,
    weekSessions
}: CompleteWorkoutModalProps) {
    const [performances, setPerformances] = useState<Map<string, ExercisePerformance>>(new Map());
    const [isSubmitting, setIsSubmitting] = useState(false);

    useEffect(() => {
        if (isOpen && exercises.length > 0) {
            console.log('Exercises received:', exercises);
            const initialPerformances = new Map<string, ExercisePerformance>();
            exercises.forEach(exercise => {
                console.log('Processing exercise:', exercise);
                const suggestedSets = generateSuggestedSets(exercise);
                console.log('Generated sets:', suggestedSets);
                initialPerformances.set(exercise.id || exercise.Id, {
                    exerciseId: exercise.id || exercise.Id,
                    exerciseName: exercise.exerciseName || exercise.ExerciseName,
                    sets: suggestedSets,
                    performanceResult: 'Success'
                });
            });
            setPerformances(initialPerformances);
        }
    }, [isOpen, exercises]);

    const getProp = (obj: any, propName: string) => {
        if (!obj) return undefined;
        const lowerProp = propName.toLowerCase();
        const key = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
        return key ? obj[key] : undefined;
    };

    const generateSuggestedSets = (exercise: any): CompletedSet[] => {
        const exerciseName = getProp(exercise, 'exerciseName') || '';
        
        // Try to get sets from week sessions first
        if (weekSessions && weekSessions[day]) {
            const daySessions = weekSessions[day];
            const exerciseSession = daySessions.find((s: any) => 
                (getProp(s, 'exerciseName') || '').toLowerCase() === exerciseName.toLowerCase()
            );
            
            if (exerciseSession) {
                const sessionSets = getProp(exerciseSession, 'sets') || [];
                if (sessionSets.length > 0) {
                    console.log('Using week session sets for', exerciseName, ':', sessionSets);
                    return sessionSets.map((s: any) => ({
                        WeightKg: getProp(s, 'weightKg') || 20,
                        Reps: getProp(s, 'reps') || 8
                    }));
                }
            }
        }

        // Fallback to old logic if week sessions not available
        const sets: CompletedSet[] = [];
        const numberOfSets = getProp(exercise, 'numberOfSets') || 3;
        const exerciseDetail = getProp(exercise, 'exerciseDetail') || getProp(exercise, 'progression');

        console.log('Week sessions not available, using fallback for:', exerciseName);

        if (!exerciseDetail) {
            console.warn('No exercise detail/progression found, using defaults');
            for (let i = 0; i < numberOfSets; i++) {
                sets.push({ WeightKg: 20, Reps: 8 });
            }
            return sets;
        }

        const program = getProp(exerciseDetail, 'program') || getProp(exerciseDetail, 'programType');

        if (program === 'Average2SavageRepsPerSet' || program === 'RepsPerSet') {
            const startingWeight = getProp(exerciseDetail, 'startingWeight') || 20;
            const targetReps = getProp(exerciseDetail, 'targetReps') || getProp(exerciseDetail, 'repRange')?.targetReps || 8;
            for (let i = 0; i < numberOfSets; i++) {
                sets.push({ WeightKg: startingWeight, Reps: targetReps });
            }
        } else {
            const startingWeight = getProp(exerciseDetail, 'startingWeight') || 20;
            const targetReps = getProp(exerciseDetail, 'targetReps') || 8;
            for (let i = 0; i < numberOfSets; i++) {
                sets.push({ WeightKg: startingWeight, Reps: targetReps });
            }
        }
        return sets;
    };

    const updateSet = (exerciseId: string, setIndex: number, field: 'WeightKg' | 'Reps', value: number) => {
        setPerformances(prev => {
            const newMap = new Map(prev);
            const perf = newMap.get(exerciseId);
            if (perf) {
                const newSets = [...perf.sets];
                newSets[setIndex] = { ...newSets[setIndex], [field]: value };
                newMap.set(exerciseId, { ...perf, sets: newSets });
            }
            return newMap;
        });
    };

    const addSet = (exerciseId: string) => {
        setPerformances(prev => {
            const newMap = new Map(prev);
            const perf = newMap.get(exerciseId);
            if (perf && perf.sets.length > 0) {
                const lastSet = perf.sets[perf.sets.length - 1];
                newMap.set(exerciseId, {
                    ...perf,
                    sets: [...perf.sets, { ...lastSet }]
                });
            }
            return newMap;
        });
    };

    const removeSet = (exerciseId: string, setIndex: number) => {
        setPerformances(prev => {
            const newMap = new Map(prev);
            const perf = newMap.get(exerciseId);
            if (perf && perf.sets.length > 1) {
                const newSets = perf.sets.filter((_, i) => i !== setIndex);
                newMap.set(exerciseId, { ...perf, sets: newSets });
            }
            return newMap;
        });
    };

    const togglePerformanceResult = (exerciseId: string) => {
        setPerformances(prev => {
            const newMap = new Map(prev);
            const perf = newMap.get(exerciseId);
            if (perf) {
                newMap.set(exerciseId, {
                    ...perf,
                    performanceResult: perf.performanceResult === 'Success' ? 'Failed' : 'Success'
                });
            }
            return newMap;
        });
    };

    const handleSubmit = async () => {
        setIsSubmitting(true);
        try {
            const performanceArray = Array.from(performances.values()).map(p => ({
                ExerciseId: p.exerciseId,
                CompletedSets: p.sets,
                PerformanceResult: p.performanceResult
            }));
            await onComplete(performanceArray);
            onClose();
        } catch (error) {
            console.error('Failed to complete workout:', error);
            alert('Failed to complete workout. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
                {/* Background overlay */}
                <div
                    className="fixed inset-0 transition-opacity bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75"
                    onClick={onClose}
                />

                {/* Modal panel */}
                <div className="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full">
                    {/* Header */}
                    <div className="bg-gradient-to-r from-indigo-600 to-purple-600 px-6 py-4">
                        <div className="flex items-center justify-between">
                            <div>
                                <h3 className="text-xl font-bold text-white">
                                    Complete Workout
                                </h3>
                                <p className="text-sm text-white/90 mt-1">
                                    {workoutName} • Week {week}, Day {day}
                                </p>
                            </div>
                            <button
                                onClick={onClose}
                                className="text-white hover:bg-white/20 rounded-lg p-2 transition-colors"
                            >
                                <X className="h-6 w-6" />
                            </button>
                        </div>
                    </div>

                    {/* Content */}
                    <div className="px-6 py-4 max-h-[60vh] overflow-y-auto">
                        <div className="space-y-6">
                            {Array.from(performances.values()).map((performance, index) => (
                                <div
                                    key={performance.exerciseId}
                                    className="border border-gray-200 dark:border-gray-700 rounded-lg p-4 bg-gray-50 dark:bg-gray-700/50"
                                >
                                    <div className="flex items-center justify-between mb-3">
                                        <div className="flex-1">
                                            <h4 className="font-bold text-gray-900 dark:text-white">
                                                {index + 1}. {performance.exerciseName}
                                            </h4>
                                        </div>
                                        <button
                                            onClick={() => togglePerformanceResult(performance.exerciseId)}
                                            className={`flex items-center px-3 py-1 rounded-full text-xs font-semibold transition-colors ${performance.performanceResult === 'Success'
                                                ? 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 hover:bg-green-200'
                                                : 'bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400 hover:bg-red-200'
                                                }`}
                                        >
                                            {performance.performanceResult === 'Success' ? (
                                                <><CheckCircle className="h-3 w-3 mr-1" /> Success</>
                                            ) : (
                                                <><XCircle className="h-3 w-3 mr-1" /> Failed</>
                                            )}
                                        </button>
                                    </div>

                                    <div className="space-y-2">
                                        {performance.sets.map((set, setIndex) => (
                                            <div key={setIndex} className="flex items-center gap-2">
                                                <span className="text-xs font-semibold text-gray-600 dark:text-gray-400 w-12">
                                                    Set {setIndex + 1}
                                                </span>
                                                <input
                                                    type="number"
                                                    step="0.5"
                                                    value={set.WeightKg}
                                                    onChange={(e) => updateSet(performance.exerciseId, setIndex, 'WeightKg', parseFloat(e.target.value) || 0)}
                                                    className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                                    placeholder="Weight (kg)"
                                                />
                                                <span className="text-xs text-gray-500 dark:text-gray-400">×</span>
                                                <input
                                                    type="number"
                                                    value={set.Reps}
                                                    onChange={(e) => updateSet(performance.exerciseId, setIndex, 'Reps', parseInt(e.target.value) || 0)}
                                                    className="flex-1 px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg text-sm bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                                                    placeholder="Reps"
                                                />
                                                {performance.sets.length > 1 && (
                                                    <button
                                                        onClick={() => removeSet(performance.exerciseId, setIndex)}
                                                        className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg transition-colors"
                                                    >
                                                        <Trash2 className="h-4 w-4" />
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                    </div>

                                    <button
                                        onClick={() => addSet(performance.exerciseId)}
                                        className="mt-2 w-full py-2 border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg text-sm text-gray-600 dark:text-gray-400 hover:border-indigo-500 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors flex items-center justify-center"
                                    >
                                        <Plus className="h-4 w-4 mr-1" />
                                        Add Set
                                    </button>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="bg-gray-50 dark:bg-gray-700/50 px-6 py-4 flex items-center justify-between border-t border-gray-200 dark:border-gray-700">
                        <button
                            onClick={onClose}
                            className="px-4 py-2 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-lg transition-colors"
                        >
                            Cancel
                        </button>
                        <button
                            onClick={handleSubmit}
                            disabled={isSubmitting}
                            className="px-6 py-2 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-lg hover:from-indigo-700 hover:to-purple-700 transition-all shadow-md flex items-center font-semibold disabled:opacity-50"
                        >
                            <Send className="h-4 w-4 mr-2" />
                            {isSubmitting ? 'Submitting...' : 'Complete Workout'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
