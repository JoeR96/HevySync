using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class WeeklyExercisePlanConfiguration : IEntityTypeConfiguration<WeeklyExercisePlan>
{
    public void Configure(EntityTypeBuilder<WeeklyExercisePlan> builder)
    {
        builder.ToTable("WeeklyExercisePlans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever(); // We generate IDs in the domain

        builder.Property(p => p.WorkoutId)
            .IsRequired();

        builder.Property(p => p.ExerciseId)
            .IsRequired();

        builder.Property(p => p.Week)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Store planned sets as JSON in a single column
        // Configure the backing field and make it accessible via the public property
        builder.OwnsMany<Set>("_plannedSets", sets =>
        {
            sets.ToJson("PlannedSets");

            sets.Property(s => s.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .IsRequired();

            sets.Property(s => s.Reps)
                .IsRequired();
        });

        // Map the public property to use the backing field
        builder.Navigation("_plannedSets")
            .HasField("_plannedSets")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore the public PlannedSets property (it's read-only and just exposes _plannedSets)
        builder.Ignore(p => p.PlannedSets);

        // Ignore domain events collection
        builder.Ignore(p => p.DomainEvents);

        // Create composite index for efficient querying
        builder.HasIndex(p => new { p.WorkoutId, p.Week });
        builder.HasIndex(p => new { p.ExerciseId, p.Week }).IsUnique();
    }
}
