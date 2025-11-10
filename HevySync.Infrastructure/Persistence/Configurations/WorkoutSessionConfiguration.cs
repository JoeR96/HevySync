using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.ToTable("WorkoutSessions");

        builder.HasKey(ws => ws.Id);

        builder.Property(ws => ws.Id)
            .ValueGeneratedNever(); // We generate IDs in the domain

        builder.Property(ws => ws.WorkoutId)
            .IsRequired();

        builder.Property(ws => ws.Week)
            .IsRequired();

        builder.Property(ws => ws.Day)
            .IsRequired();

        builder.Property(ws => ws.CompletedAt)
            .IsRequired();

        // Configure relationship with exercise performances using backing field
        builder.HasMany<SessionExercisePerformance>("_exercisePerformances")
            .WithOne()
            .HasForeignKey(ep => ep.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore the public ExercisePerformances property
        builder.Ignore(ws => ws.ExercisePerformances);

        // Ignore domain events collection
        builder.Ignore(ws => ws.DomainEvents);

        // Create index for querying by workout and week/day
        builder.HasIndex(ws => new { ws.WorkoutId, ws.Week, ws.Day });
    }
}

public class SessionExercisePerformanceConfiguration : IEntityTypeConfiguration<SessionExercisePerformance>
{
    public void Configure(EntityTypeBuilder<SessionExercisePerformance> builder)
    {
        builder.ToTable("SessionExercisePerformances");

        builder.HasKey(sep => sep.Id);

        builder.Property(sep => sep.Id)
            .ValueGeneratedNever();

        builder.Property(sep => sep.WorkoutSessionId)
            .IsRequired();

        builder.Property(sep => sep.ExerciseId)
            .IsRequired();

        builder.Property(sep => sep.Result)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Store sets as JSON in a single column (complex types can be owned or stored as JSON)
        builder.OwnsMany<Set>("_completedSets", sets =>
        {
            sets.ToJson("CompletedSets");

            sets.Property(s => s.WeightKg)
                .HasColumnType("decimal(5, 2)")
                .IsRequired();

            sets.Property(s => s.Reps)
                .IsRequired();
        });

        // Ignore the public CompletedSets property
        builder.Ignore(sep => sep.CompletedSets);
    }
}
