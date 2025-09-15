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
using Microsoft.EntityFrameworkCore;

namespace HevySync.Endpoints.Average2Savage;

internal static class Average2SavageHandler
{
    public static void MapAverage2Savage(this RouteGroupBuilder routes)
    {
        routes.MapPost("/workout", PostAverage2Savage)
            .RequireAuthorization();

        routes.MapPost("/workout/create-week-one", PostAverage2SavageCreateWorkoutWeekOneSessionExercises)
            .RequireAuthorization();

        routes.MapGet("/workout/get-current-workout", GetCurrentAverage2SavageWorkoutSessionExercises)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetCurrentAverage2SavageWorkoutSessionExercises(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] HevySyncDbContext dbContext,
        [FromQuery] Guid workoutId)
    {
        var user = await userManager.GetUserAsync(userPrincipal);

        var workout = await dbContext.Workouts
            .Where(w => w.Id == workoutId && w.ApplicationUserId == user.Id)
            .Include(w => w.WorkoutActivity)
            .Include(w => w.Exercises)
            .ThenInclude(e => e.ExerciseDetail)
            .FirstOrDefaultAsync();

        if (workout == null) return Results.NotFound($"Workout with ID {workoutId} not found or access denied.");

        var sessionExercises = await dbContext.SessionExercises
            .Where(se => workout.Exercises.Select(e => e.Id).Contains(se.ExerciseId))
            .Include(se => se.Exercise)
            .ThenInclude(e => e.ExerciseDetail)
            .Include(se => se.Sets)
            .ToListAsync();

        if (!sessionExercises.Any())
        {
            var basicWorkoutDto = workout.ToDto();

            return Results.Ok(basicWorkoutDto);
        }

        var dailyWorkouts = sessionExercises
            .GroupBy(se => se.Exercise.Day)
            .OrderBy(g => g.Key)
            .Select(dayGroup => dayGroup.ToDto())
            .ToList();


        var weeklyWorkoutPlanDto = new WeeklyWorkoutPlanDto
        {
            WorkoutId = workout.Id,
            WorkoutName = workout.Name,
            Week = workout.WorkoutActivity.Week,
            DailyWorkouts = dailyWorkouts
        };

        return Results.Ok(weeklyWorkoutPlanDto);
    }

    private static async Task<IResult> PostAverage2SavageCreateWorkoutWeekOneSessionExercises(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<SyncHevyWorkoutsRequest> validator,
        [FromBody] SyncHevyWorkoutsRequest request,
        [FromServices] HevySyncDbContext dbContext,
        [FromServices] WorkoutService workoutService
    )
    {
        var weekOne = await workoutService.CreateWorkoutWeekOneAsync(request);
        var weekOneDto = weekOne.ToDto();

        return Results.Ok(weekOneDto);
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
                    RestTimer = exerciseRequest.RestTimer,
                    ExerciseDetail = (exerciseRequest.ExerciseDetailsRequest switch
                    {
                        RepsPerSetExerciseDetailsRequest reps => new RepsPerSet
                        {
                            MinimumReps = reps.MinimumReps,
                            TargetReps = reps.TargetReps,
                            MaximumTargetReps = reps.MaximumTargetReps,
                            StartingSetCount = reps.NumberOfSets,
                            TargetSetCount = reps.TotalNumberOfSets,
                            StartingWeight = Math.Round(reps.StartingWeight, 2)
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

            var workoutDto = workout.ToDto();

            return Results.Ok(workoutDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}