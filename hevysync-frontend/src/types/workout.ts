import type {CreateExerciseRequest, ExerciseDto, ExercisePerformanceDto, SessionExerciseDto} from './exercise';

export interface CreateWorkoutRequest {
    WorkoutName: string;
    WorkoutDaysInWeek: number;
    Exercises: CreateExerciseRequest[];
}

export interface WorkoutActivityDto {
    Id?: string;
    WorkoutId?: string;
    Week: number;
    Day: number;
    WorkoutsInWeek: number;
    Status?: string;
    StartedAt?: string;
    CompletedAt?: string;
}

export interface WorkoutDto {
    Id: string;
    Name: string;
    WorkoutActivity: WorkoutActivityDto;
    Exercises: ExerciseDto[];
}

export interface GenerateWeekOneRequest {
    WorkoutId: string;
}

export interface CompleteWorkoutDayRequest {
    WorkoutId: string;
    ExercisePerformances: ExercisePerformanceDto[];
}

export interface CompleteWorkoutDayResponse {
    WorkoutId: string;
    CompletedWeek: number;
    CompletedDay: number;
    NewWeek: number;
    NewDay: number;
    WeekCompleted: boolean;
}

export interface GenerateNextWeekRequest {
    WorkoutId: string;
    WeekPerformances: ExercisePerformanceDto[];
}

export type WeekOneSessionsDto = Record<number, SessionExerciseDto[]>;
