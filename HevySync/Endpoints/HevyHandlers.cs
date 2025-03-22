using System.Security.Claims;
using HevySync.Endpoints.Requests;
using HevySync.Identity;
using HevySync.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HevySync.Endpoints;

internal static class HevyHandlers
{
    public static RouteGroupBuilder MapHevy(this RouteGroupBuilder routes)
    {
        routes.MapPost("/set-key", PostHevyKey);
        routes.MapGet("workouts", GetWorkouts);
        routes.RequireAuthorization();
        return routes;
    }

    private static async Task<IResult> PostHevyKey(
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal userPrincipal,
        HevyApiKeyRequest heavyApiKeyRequest)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        user.HevyApiKey = heavyApiKeyRequest.HevyApiKey;

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