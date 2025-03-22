using HevySync.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Data;

public class HevySyncDbContext(DbContextOptions<HevySyncDbContext> options)
    : IdentityDbContext<ApplicationUser>(options);