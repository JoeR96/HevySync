import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useWorkoutStore } from '../store/workoutStore';
import type { ExerciseDto, CompletedSetDto, ExercisePerformanceDto } from '../types/exercise';
import { PerformanceResult, ExerciseProgram } from '../types/exercise';
import { Dumbbell, Clock, CheckCircle, XCircle, Plus, Trash2, ArrowLeft, Trophy, Activity } from 'lucide-react';

interface ExercisePerformance {
  exerciseId: string;
  sets: CompletedSetDto[];
  performanceResult: PerformanceResult;
}

export default function WorkoutExecutionPage() {
  const navigate = useNavigate();
  const { currentWorkout, isLoading, completeDay, fetchWorkouts } = useWorkoutStore();
  const [performances, setPerformances] = useState<Map<string, ExercisePerformance>>(new Map());
  const [activeTimer, setActiveTimer] = useState<number | null>(null);
  const [timeRemaining, setTimeRemaining] = useState<number>(0);
  const [completingWorkout, setCompletingWorkout] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('authToken');
    if (!token) {
      navigate('/login');
      return;
    }

    if (!currentWorkout) {
      fetchWorkouts();
    }
  }, [navigate, currentWorkout, fetchWorkouts]);

  useEffect(() => {
    if (currentWorkout) {
      const todaysExercises = currentWorkout.Exercises.filter(
        e => e.Day === currentWorkout.WorkoutActivity.Day
      );

      const initialPerformances = new Map<string, ExercisePerformance>();
      todaysExercises.forEach(exercise => {
        const suggestedSets = generateSuggestedSets(exercise);
        initialPerformances.set(exercise.Id, {
          exerciseId: exercise.Id,
          sets: suggestedSets,
          performanceResult: PerformanceResult.Success
        });
      });
      setPerformances(initialPerformances);
    }
  }, [currentWorkout]);

  useEffect(() => {
    if (activeTimer !== null && timeRemaining > 0) {
      const interval = setInterval(() => {
        setTimeRemaining(prev => {
          if (prev <= 1) {
            setActiveTimer(null);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);
      return () => clearInterval(interval);
    }
  }, [activeTimer, timeRemaining]);

  const generateSuggestedSets = (exercise: ExerciseDto): CompletedSetDto[] => {
    // Use PlannedSets if available from the backend
    if (exercise.PlannedSets && exercise.PlannedSets.length > 0) {
      return exercise.PlannedSets.map(set => ({
        WeightKg: set.WeightKg,
        Reps: set.Reps
      }));
    }

    // Fallback to generating sets if PlannedSets not available
    const sets: CompletedSetDto[] = [];

    if (exercise.ExerciseDetail.Program === ExerciseProgram.Average2SavageRepsPerSet) {
      const detail = exercise.ExerciseDetail;
      for (let i = 0; i < exercise.NumberOfSets; i++) {
        sets.push({
          WeightKg: detail.StartingWeight,
          Reps: detail.TargetReps
        });
      }
    } else if (exercise.ExerciseDetail.Program === ExerciseProgram.Average2SavageHypertrophy) {
      const detail = exercise.ExerciseDetail;
      const weight = detail.TrainingMax * 0.7; // 70% of training max as default
      for (let i = 0; i < exercise.NumberOfSets; i++) {
        sets.push({
          WeightKg: Math.round(weight * 2) / 2, // Round to nearest 0.5
          Reps: 8
        });
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
          performanceResult: perf.performanceResult === PerformanceResult.Success
            ? PerformanceResult.Failed
            : PerformanceResult.Success
        });
      }
      return newMap;
    });
  };

  const startRestTimer = (seconds: number) => {
    setActiveTimer(seconds);
    setTimeRemaining(seconds);
  };

  const handleCompleteWorkout = async () => {
    if (!currentWorkout) return;

    setCompletingWorkout(true);
    try {
      const performanceArray: ExercisePerformanceDto[] = Array.from(performances.values()).map(p => ({
        ExerciseId: p.exerciseId,
        CompletedSets: p.sets,
        PerformanceResult: p.performanceResult
      }));

      await completeDay(currentWorkout.Id, performanceArray);
      navigate('/dashboard');
    } catch (error) {
      console.error('Failed to complete workout:', error);
      alert('Failed to complete workout. Please try again.');
    } finally {
      setCompletingWorkout(false);
    }
  };

  if (isLoading || !currentWorkout) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-indigo-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading workout...</p>
        </div>
      </div>
    );
  }

  const todaysExercises = currentWorkout.Exercises.filter(
    e => e.Day === currentWorkout.WorkoutActivity.Day
  ).sort((a, b) => a.Order - b.Order);

  if (todaysExercises.length === 0) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100">
        <div className="text-center">
          <Activity className="h-16 w-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">No Exercises Today</h2>
          <p className="text-gray-600 mb-6">There are no exercises scheduled for this day.</p>
          <button
            onClick={() => navigate('/dashboard')}
            className="px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-xl hover:from-indigo-700 hover:to-purple-700 transition-all shadow-md"
          >
            Back to Dashboard
          </button>
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
              <button
                onClick={() => navigate('/dashboard')}
                className="mr-4 p-2 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <ArrowLeft className="h-6 w-6 text-gray-600" />
              </button>
              <Dumbbell className="h-8 w-8 text-indigo-600 mr-3" />
              <div>
                <h1 className="text-xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent">
                  {currentWorkout.Name}
                </h1>
                <p className="text-sm text-gray-600">
                  Week {currentWorkout.WorkoutActivity.Week} • Day {currentWorkout.WorkoutActivity.Day}
                </p>
              </div>
            </div>
            <div className="flex items-center">
              <button
                onClick={handleCompleteWorkout}
                disabled={completingWorkout}
                className="px-6 py-3 bg-gradient-to-r from-green-600 to-green-700 text-white rounded-xl hover:from-green-700 hover:to-green-800 transition-all shadow-md flex items-center disabled:opacity-50"
              >
                <Trophy className="mr-2 h-5 w-5" />
                {completingWorkout ? 'Completing...' : 'Complete Workout'}
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Rest Timer */}
      {activeTimer !== null && timeRemaining > 0 && (
        <div className="bg-gradient-to-r from-orange-500 to-orange-600 text-white py-4 shadow-lg">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div className="flex items-center justify-between">
              <div className="flex items-center">
                <Clock className="h-6 w-6 mr-3 animate-pulse" />
                <div>
                  <p className="text-sm font-medium opacity-90">Rest Timer</p>
                  <p className="text-2xl font-bold">{Math.floor(timeRemaining / 60)}:{(timeRemaining % 60).toString().padStart(2, '0')}</p>
                </div>
              </div>
              <button
                onClick={() => setActiveTimer(null)}
                className="px-4 py-2 bg-white/20 rounded-lg hover:bg-white/30 transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {todaysExercises.map((exercise, exerciseIndex) => {
            const performance = performances.get(exercise.Id);
            if (!performance) return null;

            return (
              <div key={exercise.Id} className="bg-white rounded-2xl shadow-lg overflow-hidden">
                {/* Exercise Header */}
                <div className="bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white">
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3 mb-2">
                        <span className="bg-white/20 px-3 py-1 rounded-full text-sm font-medium">
                          Exercise {exerciseIndex + 1} of {todaysExercises.length}
                        </span>
                        <button
                          onClick={() => togglePerformanceResult(exercise.Id)}
                          className={`flex items-center px-3 py-1 rounded-full text-sm font-medium transition-colors ${
                            performance.performanceResult === PerformanceResult.Success
                              ? 'bg-green-500 hover:bg-green-600'
                              : 'bg-red-500 hover:bg-red-600'
                          }`}
                        >
                          {performance.performanceResult === PerformanceResult.Success ? (
                            <><CheckCircle className="h-4 w-4 mr-1" /> Success</>
                          ) : (
                            <><XCircle className="h-4 w-4 mr-1" /> Failed</>
                          )}
                        </button>
                      </div>
                      <h3 className="text-2xl font-bold">{exercise.ExerciseName}</h3>
                      <div className="flex items-center mt-2 space-x-4 text-sm opacity-90">
                        <span className="flex items-center">
                          <Dumbbell className="h-4 w-4 mr-1" />
                          {exercise.NumberOfSets} sets planned
                        </span>
                        <span className="flex items-center">
                          <Clock className="h-4 w-4 mr-1" />
                          {exercise.RestTimer}s rest
                        </span>
                      </div>
                    </div>
                    <button
                      onClick={() => startRestTimer(exercise.RestTimer)}
                      className="px-6 py-3 bg-white/20 hover:bg-white/30 rounded-xl transition-colors flex items-center"
                    >
                      <Clock className="h-5 w-5 mr-2" />
                      Start Rest Timer
                    </button>
                  </div>
                </div>

                {/* Sets Table */}
                <div className="p-6">
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-gray-200">
                          <th className="text-left py-3 px-4 text-sm font-semibold text-gray-700">Set</th>
                          <th className="text-center py-3 px-4 text-sm font-semibold text-gray-700">Weight (kg)</th>
                          <th className="text-center py-3 px-4 text-sm font-semibold text-gray-700">Reps</th>
                          <th className="text-center py-3 px-4 text-sm font-semibold text-gray-700">Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {performance.sets.map((set, setIndex) => (
                          <tr key={setIndex} className="border-b border-gray-100 hover:bg-gray-50">
                            <td className="py-4 px-4">
                              <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-indigo-100 text-indigo-700 font-semibold">
                                {setIndex + 1}
                              </span>
                            </td>
                            <td className="py-4 px-4">
                              <input
                                type="number"
                                step="0.5"
                                value={set.WeightKg}
                                onChange={(e) => updateSet(exercise.Id, setIndex, 'WeightKg', parseFloat(e.target.value) || 0)}
                                className="w-32 px-4 py-2 border border-gray-300 rounded-lg text-center focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                              />
                            </td>
                            <td className="py-4 px-4">
                              <input
                                type="number"
                                value={set.Reps}
                                onChange={(e) => updateSet(exercise.Id, setIndex, 'Reps', parseInt(e.target.value) || 0)}
                                className="w-32 px-4 py-2 border border-gray-300 rounded-lg text-center focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                              />
                            </td>
                            <td className="py-4 px-4 text-center">
                              {performance.sets.length > 1 && (
                                <button
                                  onClick={() => removeSet(exercise.Id, setIndex)}
                                  className="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                >
                                  <Trash2 className="h-5 w-5" />
                                </button>
                              )}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <button
                    onClick={() => addSet(exercise.Id)}
                    className="mt-4 w-full py-3 border-2 border-dashed border-gray-300 rounded-xl text-gray-600 hover:border-indigo-500 hover:text-indigo-600 transition-colors flex items-center justify-center"
                  >
                    <Plus className="h-5 w-5 mr-2" />
                    Add Another Set
                  </button>
                </div>
              </div>
            );
          })}
        </div>

        {/* Summary Footer */}
        <div className="mt-8 bg-white rounded-2xl shadow-lg p-6">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-lg font-bold text-gray-900 mb-2">Workout Summary</h3>
              <div className="flex items-center space-x-6 text-sm text-gray-600">
                <span className="flex items-center">
                  <Dumbbell className="h-4 w-4 mr-1" />
                  {todaysExercises.length} exercises
                </span>
                <span className="flex items-center">
                  <Activity className="h-4 w-4 mr-1" />
                  {Array.from(performances.values()).reduce((sum, p) => sum + p.sets.length, 0)} total sets
                </span>
                <span className="flex items-center">
                  <CheckCircle className="h-4 w-4 mr-1 text-green-600" />
                  {Array.from(performances.values()).filter(p => p.performanceResult === PerformanceResult.Success).length} successful
                </span>
              </div>
            </div>
            <button
              onClick={handleCompleteWorkout}
              disabled={completingWorkout}
              className="px-8 py-4 bg-gradient-to-r from-green-600 to-green-700 text-white rounded-xl hover:from-green-700 hover:to-green-800 transition-all shadow-md flex items-center text-lg font-semibold disabled:opacity-50"
            >
              <Trophy className="mr-2 h-6 w-6" />
              {completingWorkout ? 'Completing...' : 'Complete Workout'}
            </button>
          </div>
        </div>
      </main>
    </div>
  );
}
