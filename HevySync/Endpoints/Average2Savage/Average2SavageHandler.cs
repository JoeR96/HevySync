using System.Security.Claims;
using FluentValidation;
using HevySync.Application.Workouts.Commands.CompleteWorkoutDay;
using HevySync.Application.Workouts.Commands.CreateWorkout;
using HevySync.Application.Workouts.Commands.GenerateNextWeek;
using HevySync.Application.Workouts.Commands.GenerateWeekOne;
using HevySync.Application.Workouts.Queries.GetUserWorkouts;
using HevySync.Application.Workouts.Queries.GetWorkoutHistory;
using HevySync.Application.Workouts.Queries.GetWorkoutSession;
using HevySync.Application.Workouts.Queries.GetWorkoutWeekSessions;
using HevySync.Application.Workouts.Queries.GetCurrentCycleWeekSessions;
using HevySync.Application.Workouts.Queries.GetCurrentWeekPlannedExercises;
using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Endpoints.Average2Savage.Requests;
using HevySync.Endpoints.Average2Savage.Responses;
using HevySync.Endpoints.Responses;
using HevySync.Facades;
using HevySync.Infrastructure.Identity;
using HevySync.Models;
using HevySync.Models.Exercises;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppDto = HevySync.Application.DTOs;

namespace HevySync.Endpoints.Average2Savage;

internal static class Average2SavageHandler
{
    public static void MapAverage2Savage(this RouteGroupBuilder routes)
    {
        routes.MapGet("/workouts", GetUserWorkouts)
            .RequireAuthorization();

        routes.MapPost("/workout", PostAverage2Savage)
            .RequireAuthorization();

        routes.MapPost("/workout/create-week-one", PostAverage2SavageCreateWorkoutWeekOneSessionExercises)
            .RequireAuthorization();

        routes.MapPost("/workout/complete-day", PostAverage2SavageCompleteWorkoutDay)
            .RequireAuthorization();

        routes.MapPost("/workout/generate-next-week", PostAverage2SavageGenerateNextWeek)
            .RequireAuthorization();

        routes.MapGet("/workout/{workoutId:guid}/week-sessions", GetWorkoutWeekSessions)
            .RequireAuthorization();

        routes.MapGet("/current-cycle/week-sessions", GetCurrentCycleWeekSessions)
            .RequireAuthorization();

        routes.MapGet("/current-week/planned-exercises", GetCurrentWeekPlannedExercises)
            .RequireAuthorization();

        routes.MapGet("/workout/{workoutId:guid}/session/{week:int}/{day:int}", GetWorkoutSession)
            .RequireAuthorization();

        routes.MapGet("/workout/{workoutId:guid}/history", GetWorkoutHistory)
            .RequireAuthorization();
    }
    
    private static async Task<IResult> GetWorkoutWeekSessions(
        [FromRoute] Guid workoutId,
        [FromServices] IMediator mediator)
    {
        var query = new GetWorkoutWeekSessionsQuery(workoutId);
        var sessions = await mediator.Send(query);

        return Results.Ok(sessions);
    }

    private static async Task<IResult> GetCurrentCycleWeekSessions(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] IMediator mediator)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return Results.Unauthorized();

        var query = new GetCurrentCycleWeekSessionsQuery(user.Id);
        var sessions = await mediator.Send(query);

        if (sessions == null)
            return Results.NotFound(new { message = "No active workout found" });

        return Results.Ok(sessions);
    }

    private static async Task<IResult> GetCurrentWeekPlannedExercises(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] IMediator mediator)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return Results.Unauthorized();

        var query = new GetCurrentWeekPlannedExercisesQuery(user.Id);
        var plannedExercises = await mediator.Send(query);

        if (plannedExercises == null)
            return Results.NotFound(new { message = "No active workout found" });

        return Results.Ok(plannedExercises);
    }

    private static async Task<IResult> GetUserWorkouts(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] IMediator mediator)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return Results.Unauthorized();

        var query = new GetUserWorkoutsQuery(user.Id);
        var workouts = await mediator.Send(query);

        var workoutDtos = workouts.Select(w => new Models.WorkoutDto
        {
            Id = w.Id,
            Name = w.Name,
            WorkoutActivity = new Models.WorkoutActivityDto
            {
                Week = w.Activity.Week,
                Day = w.Activity.Day,
                WorkoutsInWeek = w.Activity.WorkoutsInWeek,
                Status = w.Activity.Status,
                StartedAt = w.Activity.StartedAt,
                CompletedAt = w.Activity.CompletedAt
            },
            Exercises = w.Exercises.Select(e => new Models.Exercises.ExerciseDto
            {
                RestTimer = e.RestTimer,
                Id = e.Id,
                Order = e.Order,
                ExerciseName = e.Name,
                Day = e.Day,
                NumberOfSets = e.NumberOfSets,
                ExerciseDetail = MapProgressionToResponseDto(e.Progression)
            }).ToList()
        }).ToList();

        return Results.Ok(workoutDtos);
    }

    private static async Task<IResult> PostAverage2SavageCreateWorkoutWeekOneSessionExercises(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<GenerateWeekOneRequest> validator,
        [FromBody] GenerateWeekOneRequest request,
        [FromServices] IMediator mediator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.BadRequest(new ValidationErrorResponse(
                validationResult.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));

        var command = new GenerateWeekOneCommand(request.WorkoutId);
        var result = await mediator.Send(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> PostAverage2Savage(
        ClaimsPrincipal userPrincipal,
        UserManager<ApplicationUser> userManager,
        [FromServices] IValidator<CreateWorkoutRequest> validator,
        [FromBody] CreateWorkoutRequest request,
        [FromServices] IMediator mediator)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null)
            return Results.Unauthorized();

        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.BadRequest(new ValidationErrorResponse(
                validationResult.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));

        var repsPerSetValidator = new RepsPerSetExerciseDetailsRequestValidator();
        var linearProgressionValidator = new LinearProgressionExerciseDetailsRequestValidator();

        for (int i = 0; i < request.Exercises.Count; i++)
        {
            var exercise = request.Exercises[i];
            if (exercise.ExerciseDetailsRequest is RepsPerSetExerciseDetailsRequest reps)
            {
                var repsValidationResult = await repsPerSetValidator.ValidateAsync(reps);
                if (!repsValidationResult.IsValid)
                    return Results.BadRequest(new ValidationErrorResponse(
                        repsValidationResult.Errors.Select(e => new ValidationError
                        {
                            Property = $"Exercises[{i}].ExerciseDetailsRequest.{e.PropertyName}",
                            Message = e.ErrorMessage
                        }).ToList()));
            }
            else if (exercise.ExerciseDetailsRequest is LinearProgressionExerciseDetailsRequest linear)
            {
                var linearValidationResult = await linearProgressionValidator.ValidateAsync(linear);
                if (!linearValidationResult.IsValid)
                    return Results.BadRequest(new ValidationErrorResponse(
                        linearValidationResult.Errors.Select(e => new ValidationError
                        {
                            Property = $"Exercises[{i}].ExerciseDetailsRequest.{e.PropertyName}",
                            Message = e.ErrorMessage
                        }).ToList()));
            }
        }

        var command = new CreateWorkoutCommand(
            request.WorkoutName,
            user.Id,
            request.WorkoutDaysInWeek,
            request.Exercises.Select(e => new CreateExerciseDto(
                e.ExerciseName,
                e.ExerciseTemplateId,
                e.RestTimer,
                e.Day,
                e.Order,
                e.ExerciseDetailsRequest is RepsPerSetExerciseDetailsRequest reps
                    ? reps.NumberOfSets
                    : 3,
                MapProgressionToDto(e.ExerciseDetailsRequest)
            )).ToList());

        var appWorkoutDto = await mediator.Send(command);

        var workoutDto = new Models.WorkoutDto
        {
            Id = appWorkoutDto.Id,
            Name = appWorkoutDto.Name,
            WorkoutActivity = new Models.WorkoutActivityDto
            {
                Week = appWorkoutDto.Activity.Week,
                Day = appWorkoutDto.Activity.Day,
                Id = Guid.NewGuid(),
                WorkoutId = appWorkoutDto.Id,
                WorkoutsInWeek = appWorkoutDto.Activity.WorkoutsInWeek
            },
            Exercises = appWorkoutDto.Exercises.Select(e => new Models.Exercises.ExerciseDto
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

    private static CreateProgressionDto MapProgressionToDto(ExerciseDetailsRequest exerciseDetailsRequest) =>
        exerciseDetailsRequest switch
        {
            RepsPerSetExerciseDetailsRequest reps => new CreateRepsPerSetDto(
                reps.MinimumReps,
                reps.TargetReps,
                reps.MaximumTargetReps,
                reps.NumberOfSets,
                reps.TotalNumberOfSets,
                Math.Round(reps.StartingWeight, 2),
                Math.Round(reps.WeightProgression, 2)),
            LinearProgressionExerciseDetailsRequest linear => new CreateLinearProgressionDto(
                linear.TrainingMax,
                Math.Round(linear.WeightProgression, 2),
                linear.AttemptsBeforeDeload,
                true),
            _ => throw new ArgumentException($"Unsupported progression type: {exerciseDetailsRequest.GetType().Name}")
        };

    private static ExerciseDetailDto MapProgressionToResponseDto(AppDto.ExerciseProgressionDto progression) =>
        progression switch
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
                TargetSetCount = rps.TargetSetCount,
                WeightProgression = rps.WeightProgression
            },
            _ => throw new ArgumentException($"Unsupported progression type: {progression.GetType().Name}")
        };

    private static async Task<IResult> PostAverage2SavageCompleteWorkoutDay(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<CompleteWorkoutDayRequest> validator,
        [FromBody] CompleteWorkoutDayRequest request,
        [FromServices] IMediator mediator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.BadRequest(new ValidationErrorResponse(
                validationResult.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));

        var command = new CompleteWorkoutDayCommand(
            request.WorkoutId,
            request.ExercisePerformances.Select(ep => new Application.Workouts.Commands.CompleteWorkoutDay.ExercisePerformanceDto(
                ep.ExerciseId,
                ep.CompletedSets.Select(cs => new Application.Workouts.Commands.CompleteWorkoutDay.CompletedSetDto(
                    cs.WeightKg,
                    cs.Reps
                )).ToList(),
                ep.PerformanceResult
            )).ToList());

        var result = await mediator.Send(command);

        return Results.Ok(new CompleteWorkoutDayResponse
        {
            WorkoutId = result.WorkoutId,
            CompletedWeek = result.CompletedWeek,
            CompletedDay = result.CompletedDay,
            NewWeek = result.NewWeek,
            NewDay = result.NewDay,
            WeekCompleted = result.WeekCompleted
        });
    }

    private static async Task<IResult> PostAverage2SavageGenerateNextWeek(
        ClaimsPrincipal userPrincipal,
        [FromServices] IValidator<GenerateNextWeekRequest> validator,
        [FromBody] GenerateNextWeekRequest request,
        [FromServices] IMediator mediator)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return Results.BadRequest(new ValidationErrorResponse(
                validationResult.Errors.Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                }).ToList()));

        var command = new GenerateNextWeekCommand(
            request.WorkoutId,
            request.WeekPerformances.Select(ep => new Application.Workouts.Commands.GenerateNextWeek.ExercisePerformanceDto(
                ep.ExerciseId,
                ep.CompletedSets.Select(cs => new Application.Workouts.Commands.GenerateNextWeek.CompletedSetDto(
                    cs.WeightKg,
                    cs.Reps
                )).ToList(),
                ep.PerformanceResult
            )).ToList());

        var result = await mediator.Send(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> GetWorkoutSession(
        [FromRoute] Guid workoutId,
        [FromRoute] int week,
        [FromRoute] int day,
        [FromServices] IMediator mediator)
    {
        var query = new GetWorkoutSessionQuery(workoutId, week, day);
        var session = await mediator.Send(query);

        if (session == null)
            return Results.NotFound();

        return Results.Ok(session);
    }

    private static async Task<IResult> GetWorkoutHistory(
        [FromRoute] Guid workoutId,
        [FromServices] IMediator mediator)
    {
        var query = new GetWorkoutHistoryQuery(workoutId);
        var history = await mediator.Send(query);

        return Results.Ok(history);
    }
}
