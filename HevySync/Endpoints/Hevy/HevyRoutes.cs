using HevySync.Endpoints.Average2Savage;

namespace HevySync.Endpoints.Hevy;

public static class HevyRoutes
{
    public const string BaseRoute = "hevy";

    private static readonly Dictionary<HevyEndpoint, string> RouteMappings = new()
    {
        { HevyEndpoint.SetKey, "/set-key" }
    };

    public static string GetRoutePath(this HevyEndpoint endpoint)
    {
        if (RouteMappings.TryGetValue(endpoint, out var path)) return path;

        throw new ArgumentOutOfRangeException(nameof(endpoint), $"No route path defined for {endpoint}");
    }

    public static string GetFullRoutePath(this HevyEndpoint endpoint)
    {
        return $"{BaseRoute}{endpoint.GetRoutePath()}";
    }
}