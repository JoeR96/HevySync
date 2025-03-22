using Microsoft.AspNetCore.Identity;

namespace HevySync.Identity;

public class ApplicationUser : IdentityUser
{
    public string HevyApiKey { get; set; } = string.Empty;
}