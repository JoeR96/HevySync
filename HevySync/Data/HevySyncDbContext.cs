using HevySync.Endpoints.Average2Savage.Enums;
using HevySync.Identity;
using HevySync.Models;
using HevySync.Models.Exercises;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Data;

public class HevySyncDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public HevySyncDbContext(DbContextOptions<HevySyncDbContext> options)
        : base(options)
    {
    }

    public DbSet<Workout> Workouts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExerciseDetail>()
            .HasDiscriminator<ExerciseProgram>("Program")
            .HasValue<RepsPerSet>(ExerciseProgram.Average2SavageRepsPerSet)
            .HasValue<LinearProgression>(ExerciseProgram.Average2SavageHypertrophy);

        modelBuilder.Entity<ExerciseDetail>()
            .Property("Program")
            .HasConversion<string>();
    }
}