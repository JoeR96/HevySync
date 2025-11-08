export enum ExerciseProgram {
  Average2SavageRepsPerSet = 'Average2SavageRepsPerSet',
  Average2SavageHypertrophy = 'Average2SavageHypertrophy'
}

export type ExerciseDetailsRequest =
  | RepsPerSetExerciseDetailsRequest
  | LinearProgressionExerciseDetailsRequest;

export interface RepsPerSetExerciseDetailsRequest {
  Program: ExerciseProgram.Average2SavageRepsPerSet;
  MinimumReps: number;
  TargetReps: number;
  MaximumTargetReps: number;
  NumberOfSets: number;
  TotalNumberOfSets: number;
  StartingWeight: number;
  WeightProgression: number;
}

export interface LinearProgressionExerciseDetailsRequest {
  Program: ExerciseProgram.Average2SavageHypertrophy;
  TrainingMax: number;
  WeightProgression: number;
  AttemptsBeforeDeload: number;
}

export interface CreateExerciseRequest {
  ExerciseName: string;
  ExerciseTemplateId: string;
  RestTimer: number;
  Day: number;
  Order: number;
  ExerciseDetailsRequest: ExerciseDetailsRequest;
}

export interface ExerciseDto {
  Id: string;
  ExerciseName: string;
  ExerciseTemplateId: string;
  RestTimer: number;
  Day: number;
  Order: number;
  NumberOfSets: number;
  ExerciseDetail: ExerciseProgressionDto;
}

export type ExerciseProgressionDto = LinearProgressionDto | RepsPerSetDto;

export interface LinearProgressionDto {
  Id: string;
  Program: ExerciseProgram.Average2SavageHypertrophy;
  TrainingMax: number;
  WeightProgression: number;
  AttemptsBeforeDeload: number;
}

export interface RepsPerSetDto {
  Id: string;
  Program: ExerciseProgram.Average2SavageRepsPerSet;
  MinimumReps: number;
  TargetReps: number;
  MaximumTargetReps: number;
  StartingSetCount: number;
  TargetSetCount: number;
  StartingWeight: number;
  WeightProgression: number;
}

export interface CompletedSetDto {
  WeightKg: number;
  Reps: number;
}

export enum PerformanceResult {
  Success = 'Success',
  Failed = 'Failed'
}

export interface ExercisePerformanceDto {
  ExerciseId: string;
  CompletedSets: CompletedSetDto[];
  PerformanceResult: PerformanceResult;
}

export interface SessionSetDto {
  WeightKg: number;
  Reps: number;
  RepsInReserve: number | null;
}

export interface SessionExerciseDto {
  ExerciseTemplateId: string;
  RestSeconds: number;
  Notes: string;
  Sets: SessionSetDto[];
}
