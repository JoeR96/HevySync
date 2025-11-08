using HevySync.Domain.Aggregates;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("Workouts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever(); // We generate IDs in the domain

        // Map WorkoutName value object
        builder.Property(w => w.Name)
            .HasConversion(
                name => name.Value,
                value => WorkoutName.Create(value))
            .HasColumnName("Name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.UserId)
            .HasColumnName("ApplicationUserId")
            .IsRequired();

        // Map WorkoutActivity value object as owned type
        builder.OwnsOne(w => w.Activity, activity =>
        {
            activity.Property(a => a.Week)
                .HasColumnName("Week")
                .IsRequired();

            activity.Property(a => a.Day)
                .HasColumnName("Day")
                .IsRequired();

            activity.Property(a => a.WorkoutsInWeek)
                .HasColumnName("WorkoutsInWeek")
                .IsRequired();
        });

        // Configure relationship with exercises using backing field
        builder.HasMany<Exercise>("_exercises")
            .WithOne()
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore the public Exercises property (it's just a wrapper for _exercises)
        builder.Ignore(w => w.Exercises);

        // Ignore domain events collection
        builder.Ignore(w => w.DomainEvents);
    }
}

