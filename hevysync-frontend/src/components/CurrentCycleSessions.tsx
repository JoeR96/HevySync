import { useEffect } from 'react';
import { useWorkoutStore } from '../store/workoutStore';
import { TrendingUp, Calendar, Dumbbell, CheckCircle, Clock } from 'lucide-react';

/**
 * CurrentCycleSessions Component - Workout Planning Context
 *
 * This component belongs to the "Workout Planning Context" in our DDD boundaries.
 * It displays the planned exercises for all days in the current week, allowing users
 * to preview their upcoming workouts.
 *
 * Bounded Context Responsibility:
 * - Fetch and display planned exercises for the current week
 * - Show which day is current vs past vs future
 * - Display planned sets (weight/reps) for each exercise
 * - Show progression type (AMRAP vs RepsPerSet)
 */
export default function CurrentCycleSessions() {
  const { currentWeekPlanned, fetchCurrentWeekPlannedExercises, isLoadingPlanned } = useWorkoutStore();

  useEffect(() => {
    fetchCurrentWeekPlannedExercises();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Only fetch once on mount

  // Helper to get property (handle both camelCase and PascalCase)
  const getProp = (obj: any, propName: string) => {
    if (!obj) return undefined;
    const lowerProp = propName.toLowerCase();
    const key = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
    return key ? obj[key] : undefined;
  };

  if (isLoadingPlanned) {
    return (
      <div className="bg-white rounded-2xl p-6 shadow-lg">
        <h3 className="text-xl font-bold text-gray-900 mb-4 flex items-center">
          <TrendingUp className="h-6 w-6 mr-2 text-indigo-600" />
          Current Week Overview
        </h3>
        <div className="flex items-center justify-center py-12">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
        </div>
      </div>
    );
  }

  if (!currentWeekPlanned) {
    return (
      <div className="bg-white rounded-2xl p-6 shadow-lg">
        <h3 className="text-xl font-bold text-gray-900 mb-4 flex items-center">
          <TrendingUp className="h-6 w-6 mr-2 text-indigo-600" />
          Current Week Overview
        </h3>
        <p className="text-gray-500 text-center py-8">No active workout found</p>
      </div>
    );
  }

  const currentWeek = getProp(currentWeekPlanned, 'week') || 0;
  const currentDay = getProp(currentWeekPlanned, 'currentDay') || 1;
  const totalDaysInWeek = getProp(currentWeekPlanned, 'totalDaysInWeek') || 5;
  const exercisesByDay = getProp(currentWeekPlanned, 'exercisesByDay') || {};

  // DEBUG: Log the entire response to console
  console.log('=== CURRENT WEEK PLANNED DATA ===');
  console.log('Full Response:', currentWeekPlanned);
  console.log('Week:', currentWeek);
  console.log('Current Day:', currentDay);
  console.log('Total Days:', totalDaysInWeek);
  console.log('Exercises By Day:', exercisesByDay);
  console.log('Days with data:', Object.keys(exercisesByDay));
  Object.keys(exercisesByDay).forEach(day => {
    console.log(`Day ${day}:`, exercisesByDay[day]);
    exercisesByDay[day]?.forEach((ex: any) => {
      console.log(`  - ${getProp(ex, 'name')}: ${getProp(ex, 'plannedSets')?.length || 0} sets`, getProp(ex, 'plannedSets'));
    });
  });

  return (
    <div className="bg-white rounded-2xl p-6 shadow-lg">
      <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
        <TrendingUp className="h-6 w-6 mr-2 text-indigo-600" />
        Current Week - Week {currentWeek}
      </h3>

      {/* DEBUG SECTION - REMOVE AFTER VERIFICATION */}
      <div className="mb-6 p-4 bg-yellow-50 border-2 border-yellow-300 rounded">
        <h4 className="font-bold text-sm mb-2">🐛 DEBUG DATA:</h4>
        <div className="text-xs space-y-1">
          <p><strong>Week:</strong> {currentWeek} | <strong>Current Day:</strong> {currentDay} | <strong>Total Days:</strong> {totalDaysInWeek}</p>
          <p><strong>Days with exercises:</strong> {Object.keys(exercisesByDay).join(', ') || 'NONE'}</p>
          {Object.keys(exercisesByDay).map(day => {
            const dayExercises = exercisesByDay[day] || [];
            return (
              <div key={day} className="mt-2 pl-2 border-l-2 border-yellow-400">
                <p className="font-semibold">Day {day}: {dayExercises.length} exercises</p>
                {dayExercises.map((ex: any, idx: number) => {
                  const sets = getProp(ex, 'plannedSets') || [];
                  return (
                    <p key={idx} className="pl-2">
                      • {getProp(ex, 'name')}: {sets.length} sets
                      {sets.length > 0 && ` - ${sets.map((s: any) => `${getProp(s, 'weightKg')}kg×${getProp(s, 'reps')}`).join(', ')}`}
                    </p>
                  );
                })}
              </div>
            );
          })}
        </div>
      </div>

      <div className="space-y-4">
        {Array.from({ length: totalDaysInWeek }, (_, i) => i + 1).map(dayNum => {
          const dayExercises = exercisesByDay[dayNum] || [];
          const isCurrentDay = dayNum === currentDay;
          const isPastDay = dayNum < currentDay;

          return (
            <div
              key={dayNum}
              className={`border rounded-xl p-4 ${
                isCurrentDay
                  ? 'border-indigo-500 bg-indigo-50'
                  : isPastDay
                  ? 'border-gray-300 bg-gray-50'
                  : 'border-gray-200'
              }`}
            >
              <div className="flex items-center justify-between mb-3">
                <h4 className="font-semibold text-lg text-gray-900 flex items-center">
                  <Calendar className="h-5 w-5 mr-2 text-indigo-600" />
                  Day {dayNum}
                  {isCurrentDay && (
                    <span className="ml-2 px-2 py-0.5 bg-indigo-600 text-white text-xs rounded-full">
                      Today
                    </span>
                  )}
                  {isPastDay && (
                    <CheckCircle className="ml-2 h-5 w-5 text-green-500" />
                  )}
                </h4>
                <span className="text-sm text-gray-500">
                  {dayExercises.length} {dayExercises.length === 1 ? 'exercise' : 'exercises'}
                </span>
              </div>

              <div className="space-y-3">
                {dayExercises.map((exercise: any) => {
                  const exerciseName = getProp(exercise, 'name');
                  const sets = getProp(exercise, 'plannedSets') || [];
                  const isCompleted = getProp(exercise, 'isCompleted') || false;
                  const progression = getProp(exercise, 'progression');
                  const programType = progression ? getProp(progression, 'programType') : null;
                  const isLinearProgression = programType === 'LinearProgression';

                  return (
                    <div
                      key={getProp(exercise, 'id')}
                      className={`border-l-4 pl-3 rounded-r p-3 ${
                        isCompleted
                          ? 'border-green-400 bg-green-50'
                          : 'border-indigo-400 bg-white'
                      }`}
                    >
                      <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center space-x-2">
                          <Dumbbell className="h-4 w-4 text-indigo-600" />
                          <span className="font-medium text-sm text-gray-900">{exerciseName}</span>
                          {isCompleted && (
                            <CheckCircle className="h-4 w-4 text-green-600" />
                          )}
                        </div>
                        <Clock className="h-4 w-4 text-gray-400" />
                      </div>

                      {/* Display progression info with working weight */}
                      {progression && (
                        <div className="text-xs text-gray-600 mb-2 space-y-1">
                          {isLinearProgression ? (
                            <>
                              <div className="flex items-center justify-between">
                                <span className="bg-purple-100 text-purple-700 px-2 py-1 rounded font-semibold">
                                  Linear Progression (AMRAP)
                                </span>
                              </div>
                              <div className="flex items-center justify-between bg-purple-50 px-2 py-1 rounded">
                                <span className="font-semibold text-purple-900">Training Max:</span>
                                <span className="font-bold text-purple-900">{getProp(progression, 'trainingMax')}kg</span>
                              </div>
                              {sets.length > 0 && (
                                <div className="flex items-center justify-between bg-indigo-50 px-2 py-1 rounded">
                                  <span className="font-semibold text-indigo-900">Working Weight:</span>
                                  <span className="font-bold text-indigo-900">{getProp(sets[0], 'weightKg')}kg</span>
                                </div>
                              )}
                            </>
                          ) : (
                            <>
                              <div className="flex items-center justify-between">
                                <span className="bg-blue-100 text-blue-700 px-2 py-1 rounded font-semibold">
                                  Reps Per Set
                                </span>
                              </div>
                              <div className="flex items-center justify-between bg-blue-50 px-2 py-1 rounded">
                                <span className="font-semibold text-blue-900">Target Reps:</span>
                                <span className="font-bold text-blue-900">{getProp(progression, 'minimumReps')}-{getProp(progression, 'targetReps')}-{getProp(progression, 'maximumReps')}</span>
                              </div>
                              {sets.length > 0 && (
                                <div className="flex items-center justify-between bg-indigo-50 px-2 py-1 rounded">
                                  <span className="font-semibold text-indigo-900">Working Weight:</span>
                                  <span className="font-bold text-indigo-900">{getProp(sets[0], 'weightKg')}kg</span>
                                </div>
                              )}
                            </>
                          )}
                        </div>
                      )}

                      {/* Display sets (completed or planned) */}
                      {sets.length > 0 ? (
                        <div className="space-y-1">
                          <div className="text-xs font-semibold text-gray-700 mb-1">Sets:</div>
                          <div className="flex flex-wrap gap-2">
                            {sets.map((set: any, idx: number) => {
                              const weight = getProp(set, 'weightKg');
                              const reps = getProp(set, 'reps');
                              const isLastSet = idx === sets.length - 1 && isLinearProgression && !isCompleted;

                              return (
                                <div
                                  key={idx}
                                  className={`px-3 py-1 rounded-full text-xs font-medium ${
                                    isCompleted
                                      ? 'bg-green-100 text-green-800 border border-green-300'
                                      : isLastSet
                                      ? 'bg-purple-100 text-purple-700 border border-purple-300'
                                      : 'bg-gray-100 text-gray-700'
                                  }`}
                                >
                                  {weight}kg × {reps} {isLastSet && '(AMRAP)'}
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      ) : (
                        <p className="text-xs text-gray-500 italic">
                          {isCompleted ? 'No sets recorded' : 'No planned sets available'}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
