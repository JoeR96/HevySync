using System.Security.Claims;
using HevySync.Data;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.Identity;
using HevySync.Models;
using HevySync.Models.Exercises;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HevySync.Endpoints.Average2Savage;

internal static class Average2SavageHandler
{
    public static void MapAverage2Savage(this RouteGroupBuilder routes)
    {
        routes.MapPost("/workout", PostAverage2Savage)
            .RequireAuthorization();
    }

    private static async Task<IResult> PostAverage2Savage(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromBody] CreateWorkoutRequest request,
        [FromServices] HevySyncDbContext dbContext)
    {
        var user = await userManager.GetUserAsync(userPrincipal);

        //todo: validate request using fluent validation
        if (string.IsNullOrEmpty(request.WorkoutName) ||
            !request.Exercises.Any()) return Results.BadRequest("Invalid Workout Request.");

        var workout = new Workout
        {
            Name = request.WorkoutName,
            ApplicationUserId = user.Id,
            Exercises = request.Exercises.Select(exerciseRequest => new Exercise
            {
                ExerciseName = exerciseRequest.ExerciseName,
                Day = exerciseRequest.Day,
                Order = exerciseRequest.Order,
                ExerciseProgram = exerciseRequest.ExerciseDetailsRequest is RepsPerSetExerciseDetailsRequest
                    ? ExerciseProgram.Average2SavageRepsPerSet
                    : ExerciseProgram.Average2SavageHypertrophy,
                BodyCategory = exerciseRequest.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest lp
                    ? lp.BodyCategory
                    : default,
                EquipmentType = exerciseRequest.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest le
                    ? le.EquipmentType
                    : default,
                ExerciseDetail = (exerciseRequest.ExerciseDetailsRequest switch
                {
                    RepsPerSetExerciseDetailsRequest reps => new RepsPerSet
                    {
                        MinimumReps = reps.MinimumReps,
                        TargetReps = reps.TargetReps,
                        MaximumTargetReps = reps.MaximumTargetReps,
                        NumberOfSets = reps.NumberOfSets,
                        TotalNumberOfSets = reps.TotalNumberOfSets
                    },
                    LinearProgressionExerciseDetailsRequest linear => new LinearProgression
                    {
                        WeightProgression = linear.WeightProgression,
                        AttemptsBeforeDeload = linear.AttemptsBeforeDeload
                    },
                    _ => null
                })!
            }).ToList()
        };

        dbContext.Workouts.Add(workout);
        await dbContext.SaveChangesAsync();


        var workoutDto = new WorkoutDto
        {
            Id = workout.Id,
            Name = workout.Name,
            Exercises = workout.Exercises.Select(e => new ExerciseDto
            {
                Id = e.Id,
                Order = e.Order,
                ExerciseName = e.ExerciseName,
                Day = e.Day,
                ExerciseDetail = (e.ExerciseDetail switch
                {
                    LinearProgression lp => new LinearProgressionDto
                    {
                        Id = lp.Id,
                        WeightProgression = lp.WeightProgression,
                        AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                    },
                    RepsPerSet rps => new RepsPerSetDto
                    {
                        Id = rps.Id,
                        MinimumReps = rps.MinimumReps,
                        TargetReps = rps.TargetReps,
                        MaximumTargetReps = rps.MaximumTargetReps,
                        NumberOfSets = rps.NumberOfSets,
                        TotalNumberOfSets = rps.TotalNumberOfSets
                    },
                    _ => null
                })!
            }).ToList()
        };
        return Results.Ok(workoutDto);
    }
}