using HevySync.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<
        Workout> builder)
    {
        builder.ToTable("Workouts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(w => w.Exercises)
            .WithOne(e => e.Workout)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.WorkoutActivity)
            .WithOne(wa => wa.Workout)
            .HasForeignKey<WorkoutActivity>(wa => wa.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(w => w.ApplicationUserId)
            .IsRequired();
    }
}

public class WorkoutActivityConfiguration : IEntityTypeConfiguration<WorkoutActivity>
{
    public void Configure(EntityTypeBuilder<WorkoutActivity> builder)
    {
        builder.ToTable("WorkoutActivities");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Week)
            .IsRequired();

        builder.HasOne(wa => wa.Workout)
            .WithOne(w => w.WorkoutActivity)
            .HasForeignKey<WorkoutActivity>(wa => wa.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}