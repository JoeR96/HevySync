using System.Security.Claims;
using FluentValidation;
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
        [FromServices] IValidator<CreateWorkoutRequest> validator,
        [FromBody] CreateWorkoutRequest request,
        [FromServices] HevySyncDbContext dbContext)
    {
        var user = await userManager.GetUserAsync(userPrincipal);

        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationError
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage
            }).ToList();

            return Results.BadRequest(new ValidationErrorResponse(errors));
        }

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
                        WeightProgression = Math.Round(linear.WeightProgression, 2),
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
                        Program = ExerciseProgram.Average2SavageHypertrophy,
                        Id = lp.Id,
                        WeightProgression = lp.WeightProgression,
                        AttemptsBeforeDeload = lp.AttemptsBeforeDeload
                    },
                    RepsPerSet rps => new RepsPerSetDto
                    {
                        Program = ExerciseProgram.Average2SavageHypertrophy,
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