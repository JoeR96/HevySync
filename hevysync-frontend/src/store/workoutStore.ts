import {create} from 'zustand';
import {apiClient} from '../api/client';
import type {WorkoutDto, WeekSessionsDto, CurrentWeekPlannedExercisesDto} from '../types/workout';
import type {CreateWorkoutRequest} from "../types/workout.ts";

interface WorkoutState {
    currentWorkout: WorkoutDto | null;
    workouts: WorkoutDto[];
    weekSessions: WeekSessionsDto | null;
    currentWeekPlanned: CurrentWeekPlannedExercisesDto | null;
    isLoading: boolean;
    isLoadingPlanned: boolean; // Dedicated loading state for planned exercises
    error: string | null;
    fetchWorkouts: () => Promise<void>;
    createWorkout: (workout: CreateWorkoutRequest) => Promise<WorkoutDto>;
    getWorkout: (workoutId: string) => Promise<void>;
    completeDay: (workoutId: string, performances: any[]) => Promise<void>;
    generateNextWeek: (workoutId: string, performances: any[]) => Promise<void>;
    fetchWeekSessions: (workoutId: string) => Promise<any>;
    fetchCurrentCycleWeekSessions: () => Promise<void>;
    fetchCurrentWeekPlannedExercises: () => Promise<void>;
    clearError: () => void;
}

export const useWorkoutStore = create<WorkoutState>((set) => ({
    currentWorkout: null,
    workouts: [],
    weekSessions: null,
    currentWeekPlanned: null,
    isLoading: false,
    isLoadingPlanned: false,
    error: null,

    fetchWorkouts: async () => {
        set({isLoading: true, error: null});
        try {
            const response = await apiClient.get<any[]>('/average2savage/workouts');
            const workouts = response.data;
            // Handle both camelCase and PascalCase
            const activeWorkout = workouts.find(w =>
                (w.workoutActivity?.status === 'Active') || (w.WorkoutActivity?.Status === 'Active')
            ) || null;
            set({workouts, currentWorkout: activeWorkout, isLoading: false});
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to fetch workouts', isLoading: false});
            throw error;
        }
    },

    createWorkout: async (workout: CreateWorkoutRequest) => {
        set({isLoading: true, error: null});
        try {
            const response = await apiClient.post<WorkoutDto>('/average2savage/workout', workout);
            set((state) => ({
                currentWorkout: response.data,
                workouts: [...state.workouts, response.data],
                isLoading: false,
            }));
            return response.data;
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to create workout', isLoading: false});
            throw error;
        }
    },

    getWorkout: async (workoutId: string) => {
        set({isLoading: true, error: null});
        try {
            const response = await apiClient.get<WorkoutDto>(`/average2savage/workout/${workoutId}`);
            set({currentWorkout: response.data, isLoading: false});
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to fetch workout', isLoading: false});
            throw error;
        }
    },

    completeDay: async (workoutId: string, performances: any[]) => {
        set({isLoading: true, error: null});
        try {
            await apiClient.post(`/average2savage/workout/complete-day`, {
                WorkoutId: workoutId,
                ExercisePerformances: performances
            });
            // Refresh workouts after completing a day
            const response = await apiClient.get<WorkoutDto[]>('/average2savage/workouts');
            const workouts = response.data;
            const activeWorkout = workouts.find(w => w.WorkoutActivity.Status === 'Active') || null;
            set({workouts, currentWorkout: activeWorkout, isLoading: false});
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to complete workout day', isLoading: false});
            throw error;
        }
    },

    generateNextWeek: async (workoutId: string, performances: any[]) => {
        set({isLoading: true, error: null});
        try {
            await apiClient.post(`/average2savage/workout/generate-next-week`, {
                WorkoutId: workoutId,
                WeekPerformances: performances
            });
            // Refresh workouts after generating next week
            const response = await apiClient.get<WorkoutDto[]>('/average2savage/workouts');
            const workouts = response.data;
            const activeWorkout = workouts.find(w => w.WorkoutActivity.Status === 'Active') || null;
            set({workouts, currentWorkout: activeWorkout, isLoading: false});
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to generate next week', isLoading: false});
            throw error;
        }
    },

    fetchWeekSessions: async (workoutId: string) => {
        try {
            const response = await apiClient.get(`/average2savage/workout/${workoutId}/week-sessions`);
            return response.data;
        } catch (error: any) {
            console.error('Failed to fetch week sessions:', error);
            throw error;
        }
    },

    fetchCurrentCycleWeekSessions: async () => {
        set({isLoading: true, error: null});
        try {
            const response = await apiClient.get<WeekSessionsDto>('/average2savage/current-cycle/week-sessions');
            set({weekSessions: response.data, isLoading: false});
        } catch (error: any) {
            set({error: error.response?.data?.message || 'Failed to fetch week sessions', isLoading: false});
            throw error;
        }
    },

    fetchCurrentWeekPlannedExercises: async () => {
        // Prevent duplicate fetches while already loading
        const currentState = useWorkoutStore.getState();
        if (currentState.isLoadingPlanned) {
            console.log('Already loading planned exercises, skipping fetch');
            return;
        }

        console.log('Fetching current week planned exercises...');
        set({isLoadingPlanned: true, error: null});
        try {
            const response = await apiClient.get<CurrentWeekPlannedExercisesDto>('/average2savage/current-week/planned-exercises');
            console.log('Successfully fetched planned exercises:', response.data);
            set({currentWeekPlanned: response.data, isLoadingPlanned: false});
        } catch (error: any) {
            console.error('Failed to fetch planned exercises:', error);
            set({error: error.response?.data?.message || 'Failed to fetch current week planned exercises', isLoadingPlanned: false});
            throw error;
        }
    },

    clearError: () => set({error: null}),
}));
