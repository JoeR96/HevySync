import axios from 'axios'
import { useAuth } from './auth'

const API_BASE_URL = 'http://localhost:8080'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true
})

api.interceptors.request.use(
  (config) => {
    const token = useAuth.getState().token
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuth.getState().logout()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export interface HevyApiKeyRequest {
  hevyApiKey: string;
}

export interface HevyWorkout {
  id: string;
  title: string;
  description: string;
  start_time: string;
  end_time: string;
  updated_at: string;
  created_at: string;
  exercises: HevyExercise[];
}

export interface HevyExercise {
  index: number;
  title: string;
  notes: string;
  exercise_template_id: string;
  superset_id: string | null;
  sets: HevySet[];
}

export interface HevySet {
  index: number;
  type: string;
  weight_kg?: number;
  reps?: number;
  distance_meters?: number;
  duration_seconds?: number;
  rpe?: number;
  custom_metric?: string;
}

export interface HevyApiResponse {
  page: number;
  pageCount: number;
  events: {
    type: string;
    workout: HevyWorkout;
  }[];
}

export interface LoginResponse {
  token: string;
  expiration: string;
}

const getAuthToken = () => {
  const token = useAuth.getState().token;
  if (!token) {
    throw new Error('No authentication token available');
  }
  return token;
};

const handleLogout = () => {
  useAuth.getState().logout();
};

const handleLogin = async (email: string, password: string) => {
  try {
    const response = await axios.post(`${API_BASE_URL}/login`, {
      email,
      password,
    });
    const { token } = response.data;
    useAuth.getState().setToken(token);
    return { token };
  } catch (error) {
    useAuth.getState().logout();
    throw error;
  }
};

export const apiClient = {
  login: handleLogin,
  logout: handleLogout,
  setHevyApiKey: async (hevyApiKey: string) => {
    const response = await api.post('/hevy/set-key', { hevyApiKey });
    return response.data;
  },
  getWorkouts: async (since: Date) => {
    const response = await api.get<HevyApiResponse>('/hevy/workouts', {
      params: { since: since.toISOString() }
    });
    return response.data;
  }
};

export default api 