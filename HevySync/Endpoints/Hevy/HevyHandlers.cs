using System.Security.Claims;
using FluentValidation;
using HevySync.Endpoints.Hevy.Requests;
using HevySync.Endpoints.Responses;
using HevySync.Infrastructure.Identity;
using HevySync.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HevySync.Endpoints.Hevy;

internal static class HevyHandlers
{
    public static RouteGroupBuilder MapHevy(this RouteGroupBuilder routes)
    {
        routes.MapPost("/set-key", PostHevyKey);
        routes.MapGet("workouts", GetWorkouts);
        routes.RequireAuthorization();
        return routes;
    }

    public static async Task<IResult> PostHevyKey(
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal userPrincipal,
        HevyApiKeyRequest hevyApiKeyRequest,
        IValidator<HevyApiKeyRequest> validator)
    {
        var validationResult = await validator.ValidateAsync(hevyApiKeyRequest);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationError
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage
            }).ToList();

            return Results.BadRequest(new ValidationErrorResponse(errors));
        }

        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        user.HevyApiKey = hevyApiKeyRequest.HevyApiKey;

        await userManager.UpdateAsync(user);

        return Results.Ok($"Hevy API Key has been set for {user.UserName}");
    }

    private static async Task<IResult> GetWorkouts(
        [FromServices] HevyApiService hevyApiService,
        [FromQuery] DateTimeOffset since)
    {
        var workouts = await hevyApiService.GetWorkoutEventsAsync(since);
        return Results.Ok(workouts);
    }
}