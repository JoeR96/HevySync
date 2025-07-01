using System.Security.Claims;
using FluentValidation;
using HevySync.Data;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.Endpoints.Responses;
using HevySync.Identity;
using HevySync.Models;
using HevySync.Models.Exercises;
using HevySync.Services;
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
        [FromServices] HevySyncDbContext dbContext,
        [FromServices] WorkoutService workoutService)
    {
        try
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
                WorkoutActivity = new WorkoutActivity
                {
                    Week = 1,
                    Day = 1,
                    WorkoutsInWeek = request.WorkoutDaysInWeek
                },
                Exercises = request.Exercises.Select(exerciseRequest => new Exercise
                {
                    ExerciseTemplateId = exerciseRequest.ExerciseTemplateId,
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
                            StartingSetCount = reps.NumberOfSets,
                            TargetSetCount = reps.TotalNumberOfSets
                        },
                        LinearProgressionExerciseDetailsRequest linear => new LinearProgression
                        {
                            WeightProgression = Math.Round(linear.WeightProgression, 2),
                            AttemptsBeforeDeload = linear.AttemptsBeforeDeload,
                            TrainingMax = linear.TrainingMax
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
                WorkoutActivity = workout.WorkoutActivity,
                Exercises = workout.Exercises.Select(e => new ExerciseDto
                {
                    RestTimer = e.RestTimer,
                    Id = e.Id,
                    Order = e.Order,
                    ExerciseName = e.ExerciseName,
                    Day = e.Day,
                    NumberOfSets = e.NumberOfSets,
                    ExerciseDetail = (e.ExerciseDetail switch
                    {
                        LinearProgression lp => new LinearProgressionDto
                        {
                            Program = ExerciseProgram.Average2SavageHypertrophy,
                            Id = lp.Id,
                            WeightProgression = lp.WeightProgression,
                            AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                            TrainingMax = lp.TrainingMax
                        },
                        RepsPerSet rps => new RepsPerSetDto
                        {
                            StartingWeight = rps.StartingWeight,
                            Program = ExerciseProgram.Average2SavageHypertrophy,
                            Id = rps.Id,
                            MinimumReps = rps.MinimumReps,
                            TargetReps = rps.TargetReps,
                            MaximumTargetReps = rps.MaximumTargetReps,
                            StartingSetCount = rps.StartingSetCount,
                            TargetSetCount = rps.TargetSetCount
                        },
                        _ => null
                    })!
                }).ToList()
            };

            var routines = await workoutService.CreateHevyWorkoutWeekOneAsync(new SyncHevyWorkoutsRequest
            {
                WorkoutId = workout.Id
            });
            
            return Results.Ok(routines);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}