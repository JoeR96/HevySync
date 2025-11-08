namespace HevySync.Endpoints.Average2Savage;

public static class Average2SavageRoutes
{
    public const string BaseRoute = "average2savage";

    private static readonly Dictionary<Average2SavageEndpoint, string> RouteMappings = new()
    {
        { Average2SavageEndpoint.Workout, "/workout" },
        { Average2SavageEndpoint.WorkoutCreateWeekOne, "/workout/create-week-one" },
        { Average2SavageEndpoint.WorkoutCompleteDay, "/workout/complete-day" },
        { Average2SavageEndpoint.WorkoutGenerateNextWeek, "/workout/generate-next-week" }
    };

    public static string GetFullRoutePath(this Average2SavageEndpoint endpoint)
    {
        return $"{BaseRoute}{endpoint.GetRoutePath()}";
    }

    private static string GetRoutePath(this Average2SavageEndpoint endpoint)
    {
        if (RouteMappings.TryGetValue(endpoint, out var path)) return path;

        throw new ArgumentOutOfRangeException(nameof(endpoint), $"No route path defined for {endpoint}");
    }
}