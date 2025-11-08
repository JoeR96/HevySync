import { create } from 'zustand';
import { WorkoutDto, CreateWorkoutRequest } from '../types/workout';
import { apiClient } from '../api/client';

interface WorkoutState {
  currentWorkout: WorkoutDto | null;
  workouts: WorkoutDto[];
  isLoading: boolean;
  error: string | null;
  createWorkout: (workout: CreateWorkoutRequest) => Promise<WorkoutDto>;
  getWorkout: (workoutId: string) => Promise<void>;
  clearError: () => void;
}

export const useWorkoutStore = create<WorkoutState>((set) => ({
  currentWorkout: null,
  workouts: [],
  isLoading: false,
  error: null,

  createWorkout: async (workout: CreateWorkoutRequest) => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.post<WorkoutDto>('/api/average2savage/workout', workout);
      set((state) => ({
        currentWorkout: response.data,
        workouts: [...state.workouts, response.data],
        isLoading: false,
      }));
      return response.data;
    } catch (error: any) {
      set({ error: error.response?.data?.message || 'Failed to create workout', isLoading: false });
      throw error;
    }
  },

  getWorkout: async (workoutId: string) => {
    set({ isLoading: true, error: null });
    try {
      const response = await apiClient.get<WorkoutDto>(`/api/average2savage/workout/${workoutId}`);
      set({ currentWorkout: response.data, isLoading: false });
    } catch (error: any) {
      set({ error: error.response?.data?.message || 'Failed to fetch workout', isLoading: false });
      throw error;
    }
  },

  clearError: () => set({ error: null }),
}));
