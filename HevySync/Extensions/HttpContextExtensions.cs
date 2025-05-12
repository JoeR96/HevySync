using HevySync.Identity;

namespace HevySync.Extensions;

public static class HttpContextExtensions
{
    public static ApplicationUser? GetAuthenticatedUser(this HttpContext context)
    {
        context.Items.TryGetValue("AuthenticatedUser", out var userObj);
        return userObj as ApplicationUser;
    }
}