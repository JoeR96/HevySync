using Microsoft.AspNetCore.Identity;

namespace HevySync.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string HevyApiKey { get; set; } = string.Empty;
}