-- Insert workout sessions for Week 11, Day 1 and Day 2
-- Get exercises for each day and create sessions

-- Week 11, Day 1 Session
DO $$
DECLARE
    workout_id uuid := '76ea02f5-4873-4da9-8b10-71655d9bc2df';
    session_id_day1 uuid := gen_random_uuid();
    session_id_day2 uuid := gen_random_uuid();
    exercise_id uuid;
    performance_id uuid;
    num_sets int;
BEGIN
    -- Create session for Week 11, Day 1
    INSERT INTO "WorkoutSessions" ("Id", "WorkoutId", "Week", "Day", "CompletedAt")
    VALUES (session_id_day1, workout_id, 11, 1, NOW() - INTERVAL '2 days');

    -- Add exercise performances for Day 1
    FOR exercise_id, num_sets IN
        SELECT "Id", "NumberOfSets" FROM "Exercises"
        WHERE "WorkoutId" = workout_id AND "Day" = 1
    LOOP
        performance_id := gen_random_uuid();

        INSERT INTO "SessionExercisePerformances" ("Id", "WorkoutSessionId", "ExerciseId", "Result", "CompletedSets")
        VALUES (
            performance_id,
            session_id_day1,
            exercise_id,
            'Success',
            jsonb_build_array(
                jsonb_build_object('WeightKg', 100, 'Reps', 10),
                jsonb_build_object('WeightKg', 100, 'Reps', 10),
                jsonb_build_object('WeightKg', 100, 'Reps', 10)
            )
        );
    END LOOP;

    -- Create session for Week 11, Day 2
    INSERT INTO "WorkoutSessions" ("Id", "WorkoutId", "Week", "Day", "CompletedAt")
    VALUES (session_id_day2, workout_id, 11, 2, NOW() - INTERVAL '1 day');

    -- Add exercise performances for Day 2
    FOR exercise_id, num_sets IN
        SELECT "Id", "NumberOfSets" FROM "Exercises"
        WHERE "WorkoutId" = workout_id AND "Day" = 2
    LOOP
        performance_id := gen_random_uuid();

        INSERT INTO "SessionExercisePerformances" ("Id", "WorkoutSessionId", "ExerciseId", "Result", "CompletedSets")
        VALUES (
            performance_id,
            session_id_day2,
            exercise_id,
            'Success',
            jsonb_build_array(
                jsonb_build_object('WeightKg', 105, 'Reps', 10),
                jsonb_build_object('WeightKg', 105, 'Reps', 10),
                jsonb_build_object('WeightKg', 105, 'Reps', 10)
            )
        );
    END LOOP;

END $$;

-- Verify the sessions were created
SELECT ws."Week", ws."Day", COUNT(sep."Id") as exercise_count
FROM "WorkoutSessions" ws
LEFT JOIN "SessionExercisePerformances" sep ON sep."WorkoutSessionId" = ws."Id"
GROUP BY ws."Week", ws."Day"
ORDER BY ws."Week", ws."Day";
