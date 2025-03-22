using HevySync.Identity;
using Microsoft.AspNetCore.Identity;

namespace HevySync.Handlers;

public class HevyApiAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public HevyApiAuthHandler(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var userPrincipal = _httpContextAccessor.HttpContext?.User;

        if (userPrincipal?.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user != null && !string.IsNullOrEmpty(user.HevyApiKey))
            {
                request.Headers.Remove("api-key");
                request.Headers.Add("api-key", user.HevyApiKey);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}