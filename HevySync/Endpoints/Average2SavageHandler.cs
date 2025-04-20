using System.Security.Claims;
using HevySync.Endpoints.Requests;
using HevySync.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HevySync.Handlers;

internal static class Average2SavageHandler
{
    public static RouteGroupBuilder MapAverage2Savage(this RouteGroupBuilder routes)
    {
        routes.MapPost("/average2savage", PostAverage2Savage);
        return routes;
    }

    private static async Task<IResult> PostAverage2Savage(
        [FromBody] CreateWorkoutRequest request,
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal userPrincipal)
    {
        var user = await userManager.GetUserAsync(userPrincipal);
        if (user == null) return Results.Unauthorized();

        return Results.Ok($"Hevy API Key has been set for {user.UserName}");
    }
}

public record ExerciseRequest
{
    public int Day { get; set; }
    public string ExerciseName { get; set; }
    public WorkoutType Method { get; set; }
    public Category Category { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public Guid UserId { get; set; }
    public RepsPerSetRequest? RepsPerSet { get; set; }
    public LinearProgressionExerciseRequest? LinearProgression { get; set; }
}

public record RepsPerSetRequest
{
    public int MinimumReps { get; set; }
    public int TargetReps { get; set; }
    public int MaximumTargetReps { get; set; }
    public int NumberOfSets { get; set; }
    public int TotalNumberOfSets { get; set; }
}

public record LinearProgressionExerciseRequest
{
    public decimal WeightProgression { get; set; }
    public int AttemptsBeforeDeload { get; set; }
    public WorkoutType Method { get; set; }
    public Category Category { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public Guid UserId { get; set; }
}

public enum Category
{
    Shoulders,
    Chest,
    Back,
    Biceps,
    Triceps,
    Legs
}

public enum EquipmentType
{
    Barbell,
    SmithMachine,
    Dumbbell,
    Machine,
    Cable,
    Bodyweight
}

public enum WorkoutType
{
    Average2SavageRepsPerSet,
    Average2SavageHypertrophy
}