using Microsoft.AspNetCore.Identity;

namespace HevySync.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string HevyApiKey { get; set; } = string.Empty;
}

