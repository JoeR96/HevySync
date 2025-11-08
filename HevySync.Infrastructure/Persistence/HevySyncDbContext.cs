using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Infrastructure.Identity;
using HevySync.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HevySync.Infrastructure.Persistence;

public class HevySyncDbContext(DbContextOptions<HevySyncDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Workout> Workouts { get; set; } = null!;
    public DbSet<Exercise> Exercises { get; set; } = null!;
    public DbSet<ExerciseProgression> ExerciseProgressions { get; set; } = null!;
    public DbSet<Activity> Activities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new WorkoutConfiguration());
        modelBuilder.ApplyConfiguration(new ExerciseConfiguration());
        modelBuilder.ApplyConfiguration(new ExerciseProgressionConfiguration());
        modelBuilder.ApplyConfiguration(new LinearProgressionStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new RepsPerSetStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new ActivityConfiguration());
    }
}

