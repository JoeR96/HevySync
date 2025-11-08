using System.Security.Claims;
using FluentValidation;
using HevySync.Application.Common;
using HevySync.Application.Workouts.Commands.CompleteWorkoutDay;
using HevySync.Application.Workouts.Commands.CreateWorkout;
using HevySync.Application.Workouts.Commands.GenerateNextWeek;
using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.Endpoints.Responses;
using HevySync.Infrastructure.Identity;
using HevySync.Models;
using HevySync.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppDto = HevySync.Application.DTOs;

namespace HevySync.Endpoints.Average2Savage;

internal static class Average2SavageHandler
{
    public static void MapAverage2Savage(this RouteGroupBuilder routes)
    {
        routes.MapPost("/workout", PostAverage2Savage)
            .RequireAuthorization();

        routes.MapPost("/workout/create-week-one", PostAverage2SavageCreateWorkoutWeekOneSessionExercises)
            .RequireAuthorization();

        routes.MapPost("/workout/complete-day", PostAverage2SavageCompleteWorkoutDay)
            .RequireAuthorization();

        routes.MapPost("/workout/generate-next-week", PostAverage2SavageGenerateNextWeek)
            .RequireAuthorization();
    }

    private static async Task<IResult> PostAverage2SavageCreateWorkoutWeekOneSessionExercises(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<GenerateWeekOneRequest> validator,
        [FromBody] GenerateWeekOneRequest request,
        [FromServices] ICommandHandler<GenerateWeekOneCommand, Dictionary<int, List<Application.Workouts.Commands.GenerateWeekOne.SessionExerciseDto>>> commandHandler)
    {
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

        var command = new GenerateWeekOneCommand
        {
            WorkoutId = request.WorkoutId
        };

        var result = await commandHandler.HandleAsync(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> PostAverage2Savage(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] IValidator<CreateWorkoutRequest> validator,
        [FromBody] CreateWorkoutRequest request,
        [FromServices] ICommandHandler<CreateWorkoutCommand, AppDto.WorkoutDto> commandHandler)
    {
        try
        {
            var user = await userManager.GetUserAsync(userPrincipal);
            if (user == null)
            {
                return Results.Unauthorized();
            }

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

            // Manually validate nested ExerciseDetailsRequest objects
            var repsPerSetValidator = new RepsPerSetExerciseDetailsRequestValidator();
            var linearProgressionValidator = new LinearProgressionExerciseDetailsRequestValidator();

            for (int i = 0; i < request.Exercises.Count; i++)
            {
                var exercise = request.Exercises[i];
                if (exercise.ExerciseDetailsRequest is RepsPerSetExerciseDetailsRequest reps)
                {
                    var repsValidationResult = await repsPerSetValidator.ValidateAsync(reps);
                    if (!repsValidationResult.IsValid)
                    {
                        var errors = repsValidationResult.Errors.Select(e => new ValidationError
                        {
                            Property = $"Exercises[{i}].ExerciseDetailsRequest.{e.PropertyName}",
                            Message = e.ErrorMessage
                        }).ToList();

                        return Results.BadRequest(new ValidationErrorResponse(errors));
                    }
                }
                else if (exercise.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest linear)
                {
                    var linearValidationResult = await linearProgressionValidator.ValidateAsync(linear);
                    if (!linearValidationResult.IsValid)
                    {
                        var errors = linearValidationResult.Errors.Select(e => new ValidationError
                        {
                            Property = $"Exercises[{i}].ExerciseDetailsRequest.{e.PropertyName}",
                            Message = e.ErrorMessage
                        }).ToList();

                        return Results.BadRequest(new ValidationErrorResponse(errors));
                    }
                }
            }

            // Map request to command
            var command = new CreateWorkoutCommand
            {
                WorkoutName = request.WorkoutName,
                UserId = user.Id,
                WorkoutDaysInWeek = request.WorkoutDaysInWeek,
                Exercises = request.Exercises.Select(e => new CreateExerciseDto
                {
                    ExerciseName = e.ExerciseName,
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    RestTimer = e.RestTimer,
                    Day = e.Day,
                    Order = e.Order,
                    NumberOfSets = e.ExerciseDetailsRequest is RepsPerSetExerciseDetailsRequest reps
                        ? reps.NumberOfSets
                        : 3, // Default for linear progression
                    BodyCategory = e.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest lp
                        ? lp.BodyCategory.ToString()
                        : null,
                    EquipmentType = e.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest le
                        ? le.EquipmentType.ToString()
                        : null,
                    Progression = MapProgressionToDto(e.ExerciseDetailsRequest)
                }).ToList()
            };

            // Execute command
            var appWorkoutDto = await commandHandler.HandleAsync(command);

            // Map Application DTO to Response DTO
            var workoutDto = new WorkoutDto
            {
                Id = appWorkoutDto.Id,
                Name = appWorkoutDto.Name,
                WorkoutActivity = new WorkoutActivityDto
                {
                    Week = appWorkoutDto.Activity.Week,
                    Day = appWorkoutDto.Activity.Day,
                    Id = Guid.NewGuid(), // Not used in tests
                    WorkoutId = appWorkoutDto.Id,
                    WorkoutsInWeek = appWorkoutDto.Activity.WorkoutsInWeek
                },
                Exercises = appWorkoutDto.Exercises.Select(e => new ExerciseDto
                {
                    RestTimer = e.RestTimer,
                    Id = e.Id,
                    Order = e.Order,
                    ExerciseName = e.Name,
                    Day = e.Day,
                    NumberOfSets = e.NumberOfSets,
                    ExerciseDetail = MapProgressionToResponseDto(e.Progression)
                }).ToList()
            };

            return Results.Ok(workoutDto);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static CreateProgressionDto MapProgressionToDto(ExerciseDetailsRequest exerciseDetailsRequest)
    {
        return exerciseDetailsRequest switch
        {
            RepsPerSetExerciseDetailsRequest reps => new CreateRepsPerSetDto
            {
                ProgramType = "RepsPerSet",
                MinimumReps = reps.MinimumReps,
                TargetReps = reps.TargetReps,
                MaximumReps = reps.MaximumTargetReps,
                StartingSetCount = reps.NumberOfSets,
                TargetSetCount = reps.TotalNumberOfSets,
                StartingWeight = Math.Round(reps.StartingWeight, 2)
            },
            LinearProgressionExerciseDetailsRequest linear => new CreateLinearProgressionDto
            {
                ProgramType = "LinearProgression",
                TrainingMax = linear.TrainingMax,
                WeightProgression = Math.Round(linear.WeightProgression, 2),
                AttemptsBeforeDeload = linear.AttemptsBeforeDeload,
                IsPrimary = true // Default to primary
            },
            _ => throw new ArgumentException($"Unsupported progression type: {exerciseDetailsRequest.GetType().Name}")
        };
    }

    private static ExerciseDetailDto MapProgressionToResponseDto(AppDto.ExerciseProgressionDto progression)
    {
        return progression switch
        {
            AppDto.LinearProgressionDto lp => new LinearProgressionDto
            {
                Program = ExerciseProgram.Average2SavageHypertrophy,
                Id = lp.Id,
                WeightProgression = lp.WeightProgression,
                AttemptsBeforeDeload = lp.AttemptsBeforeDeload,
                TrainingMax = lp.TrainingMax
            },
            AppDto.RepsPerSetDto rps => new RepsPerSetDto
            {
                StartingWeight = rps.StartingWeight,
                Program = ExerciseProgram.Average2SavageRepsPerSet,
                Id = rps.Id,
                MinimumReps = rps.MinimumReps,
                TargetReps = rps.TargetReps,
                MaximumTargetReps = rps.MaximumReps,
                StartingSetCount = rps.StartingSetCount,
                TargetSetCount = rps.TargetSetCount
            },
            _ => throw new ArgumentException($"Unsupported progression type: {progression.GetType().Name}")
        };
    }

    private static async Task<IResult> PostAverage2SavageCompleteWorkoutDay(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<CompleteWorkoutDayRequest> validator,
        [FromBody] CompleteWorkoutDayRequest request,
        [FromServices] ICommandHandler<CompleteWorkoutDayCommand, CompleteWorkoutDayResult> commandHandler)
    {
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

        var command = new CompleteWorkoutDayCommand
        {
            WorkoutId = request.WorkoutId,
            ExercisePerformances = request.ExercisePerformances.Select(ep => new Application.Workouts.Commands.CompleteWorkoutDay.ExercisePerformanceDto
            {
                ExerciseId = ep.ExerciseId,
                CompletedSets = ep.CompletedSets.Select(cs => new Application.Workouts.Commands.CompleteWorkoutDay.CompletedSetDto
                {
                    WeightKg = cs.WeightKg,
                    Reps = cs.Reps
                }).ToList(),
                PerformanceResult = ep.PerformanceResult
            }).ToList()
        };

        var result = await commandHandler.HandleAsync(command);

        var response = new CompleteWorkoutDayResponse
        {
            WorkoutId = result.WorkoutId,
            CompletedWeek = result.CompletedWeek,
            CompletedDay = result.CompletedDay,
            NewWeek = result.NewWeek,
            NewDay = result.NewDay,
            WeekCompleted = result.WeekCompleted
        };

        return Results.Ok(response);
    }

    private static async Task<IResult> PostAverage2SavageGenerateNextWeek(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<GenerateNextWeekRequest> validator,
        [FromBody] GenerateNextWeekRequest request,
        [FromServices] ICommandHandler<GenerateNextWeekCommand, Dictionary<int, List<Application.Workouts.Commands.GenerateWeekOne.SessionExerciseDto>>> commandHandler)
    {
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

        var command = new GenerateNextWeekCommand
        {
            WorkoutId = request.WorkoutId,
            WeekPerformances = request.WeekPerformances.Select(ep => new Application.Workouts.Commands.GenerateNextWeek.ExercisePerformanceDto
            {
                ExerciseId = ep.ExerciseId,
                CompletedSets = ep.CompletedSets.Select(cs => new Application.Workouts.Commands.GenerateNextWeek.CompletedSetDto
                {
                    WeightKg = cs.WeightKg,
                    Reps = cs.Reps
                }).ToList(),
                PerformanceResult = ep.PerformanceResult
            }).ToList()
        };

        var result = await commandHandler.HandleAsync(command);

        return Results.Ok(result);
    }
}