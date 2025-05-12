using HevySync.Identity;
using Microsoft.AspNetCore.Identity;

namespace HevySync.Middleware;

public class UserResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserResolutionMiddleware(RequestDelegate next, UserManager<ApplicationUser> userManager)
    {
        _next = next;
        _userManager = userManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var user = await _userManager.GetUserAsync(context.User);
            if (user != null)
                context.Items["AuthenticatedUser"] = user;
        }

        await _next(context);
    }
}