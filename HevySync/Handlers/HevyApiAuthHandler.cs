using HevySync.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HevySync.Handlers;

public class HevyApiAuthHandler(IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var userPrincipal = httpContextAccessor.HttpContext?.User;

        if (userPrincipal?.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(userPrincipal);
            if (user != null && !string.IsNullOrEmpty(user.HevyApiKey))
            {
                request.Headers.Remove("api-key");
                request.Headers.Add("api-key", user.HevyApiKey);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}