import {useEffect} from 'react';
import {useWorkoutStore} from '../store/workoutStore';
import {Calendar, Dumbbell} from 'lucide-react';
import {DashboardCard} from './DashboardCard';
import type {PlannedExerciseDto, SetDto} from '../types/workout';

// Helper function to handle both PascalCase and camelCase
const getProp = (obj: any, propName: string) => {
    if (!obj) return undefined;
    const lowerProp = propName.toLowerCase();
    const key = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
    return key ? obj[key] : undefined;
};

/**
 * Weekly Exercises List Component
 * Shows all exercises for the current week with their planned sets
 */
export default function WeeklyExercisesList() {
    const {currentWeekPlanned, fetchCurrentWeekPlannedExercises, isLoadingPlanned} = useWorkoutStore();

    useEffect(() => {
        fetchCurrentWeekPlannedExercises();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only fetch once on mount

    if (isLoadingPlanned) {
        return (
            <DashboardCard>
                <div className="flex items-center justify-center py-8">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
                    <p className="ml-3 text-gray-600 dark:text-gray-400">Loading week plan...</p>
                </div>
            </DashboardCard>
        );
    }

    if (!isLoadingPlanned && !currentWeekPlanned) {
        return (
            <DashboardCard>
                <div className="text-center py-8">
                    <Calendar className="h-12 w-12 text-gray-400 mx-auto mb-3"/>
                    <p className="text-gray-600 dark:text-gray-400">No workout plan available</p>
                </div>
            </DashboardCard>
        );
    }

    const week = getProp(currentWeekPlanned, 'week') || 0;
    const currentDay = getProp(currentWeekPlanned, 'currentDay') || 1;
    const totalDaysInWeek = getProp(currentWeekPlanned, 'totalDaysInWeek') || 5;
    const exercisesByDay = getProp(currentWeekPlanned, 'exercisesByDay') || {};

    return (
        <DashboardCard>
            <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center">
                <Calendar className="h-6 w-6 mr-2 text-indigo-600 dark:text-indigo-400"/>
                This Week's Training Plan - Week {week}
            </h3>

            {/* Display days as columns */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
                {Array.from({length: totalDaysInWeek}, (_, i) => i + 1).map(dayNum => {
                    const dayExercises: PlannedExerciseDto[] = exercisesByDay[dayNum] || [];
                    const isCurrentDay = dayNum === currentDay;
                    const isPastDay = dayNum < currentDay;

                    return (
                        <div
                            key={dayNum}
                            className={`border-2 rounded-xl p-4 ${
                                isCurrentDay
                                    ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-900/20 dark:border-indigo-600'
                                    : isPastDay
                                        ? 'border-gray-300 bg-gray-50 dark:bg-gray-800 dark:border-gray-600'
                                        : 'border-gray-200 bg-white dark:bg-gray-800 dark:border-gray-700'
                            }`}
                        >
                            {/* Day header */}
                            <div className="mb-4 pb-3 border-b-2 border-gray-200 dark:border-gray-700">
                                <h4 className={`font-bold text-lg ${
                                    isCurrentDay
                                        ? 'text-indigo-600 dark:text-indigo-400'
                                        : 'text-gray-800 dark:text-gray-200'
                                }`}>
                                    Day {dayNum}
                                </h4>
                                {isCurrentDay && (
                                    <span
                                        className="mt-1 inline-block text-xs bg-indigo-600 text-white px-2 py-1 rounded-full font-semibold">
                    Today
                  </span>
                                )}
                                {isPastDay && (
                                    <span
                                        className="mt-1 inline-block text-xs bg-gray-400 dark:bg-gray-600 text-white px-2 py-1 rounded-full font-semibold">
                    Done
                  </span>
                                )}
                            </div>

                            {/* Exercises for this day */}
                            {dayExercises.length === 0 ? (
                                <p className="text-sm text-gray-400 dark:text-gray-500 text-center py-4">Rest day</p>
                            ) : (
                                <div className="space-y-3">
                                    {dayExercises.map((exercise: PlannedExerciseDto) => {
                                        const name = getProp(exercise, 'name') || 'Unknown Exercise';
                                        const plannedSets: SetDto[] = getProp(exercise, 'plannedSets') || [];
                                        const numberOfSets = getProp(exercise, 'numberOfSets') || plannedSets.length;
                                        const isCompleted = getProp(exercise, 'isCompleted') || false;
                                        const progression = getProp(exercise, 'progression');
                                        const programType = progression ? getProp(progression, 'programType') : null;
                                        const isLinearProgression = programType === 'LinearProgression';

                                        // Get unique weight/rep combinations for display
                                        const uniqueSets = plannedSets.reduce((acc: Array<{
                                            weight: number,
                                            reps: number,
                                            count: number
                                        }>, set: SetDto) => {
                                            const weight = getProp(set, 'weightKg') || 0;
                                            const reps = getProp(set, 'reps') || 0;
                                            const existing = acc.find(s => s.weight === weight && s.reps === reps);
                                            if (existing) {
                                                existing.count += 1;
                                            } else {
                                                acc.push({weight, reps, count: 1});
                                            }
                                            return acc;
                                        }, []);

                                        return (
                                            <div
                                                key={getProp(exercise, 'id')}
                                                className={`border-l-4 pl-3 py-2 ${
                                                    isCompleted
                                                        ? 'border-green-500 bg-green-50 dark:bg-green-900/20'
                                                        : 'border-indigo-400 bg-white dark:bg-gray-700/50'
                                                } rounded-r`}
                                            >
                                                {/* Exercise name */}
                                                <div className="flex items-start mb-2">
                                                    <Dumbbell
                                                        className="h-4 w-4 mr-1 mt-0.5 text-gray-600 dark:text-gray-400 flex-shrink-0"/>
                                                    <span
                                                        className="font-semibold text-gray-900 dark:text-white text-sm leading-tight">
                            {name}
                          </span>
                                                </div>

                                                {/* Program type badge */}
                                                {programType && (
                                                    <div className="mb-2">
                            <span className={`text-xs px-2 py-0.5 rounded-full inline-block ${
                                isLinearProgression
                                    ? 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-400'
                                    : 'bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400'
                            }`}>
                              {isLinearProgression ? 'LP' : 'RPS'}
                            </span>
                                                    </div>
                                                )}
                          
                                                {!isLinearProgression && plannedSets.length > 0 && (
                                                    <div className="mb-2 text-xs">
                            <span className="text-gray-600 dark:text-gray-400 font-semibold">
                              Weight:
                            </span>
                                                        <span className="text-gray-900 dark:text-white font-bold">
                              {getProp(plannedSets[0], 'weightKg')}kg
                            </span>
                                                    </div>
                                                )}

                                                {/* Sets */}
                                                {plannedSets.length > 0 ? (
                                                    <div className="space-y-1 text-xs">
                                                        {uniqueSets.map((setGroup, idx) => {
                                                            const isLastSet = idx === uniqueSets.length - 1 && isLinearProgression;
                                                            return (
                                                                <div key={idx}
                                                                     className="text-gray-700 dark:text-gray-300">
                                                                    {setGroup.count > 1 ? `${setGroup.count}×` : ''} {setGroup.weight}kg
                                                                    × {setGroup.reps}{isLastSet ? '+' : ''}
                                                                    {isLastSet && <span
                                                                        className="ml-1 text-purple-600 dark:text-purple-400 font-semibold">(AMRAP)</span>}
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                ) : (
                                                    <p className="text-xs text-gray-500 dark:text-gray-400 italic">No
                                                        sets</p>
                                                )}
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
    );
}
