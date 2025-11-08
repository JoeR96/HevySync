using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // We generate IDs in the domain

        // Map ExerciseName value object
        builder.Property(e => e.Name)
            .HasConversion(
                name => name.Value,
                value => ExerciseName.Create(value))
            .HasColumnName("ExerciseName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.ExerciseTemplateId)
            .IsRequired();

        // Map RestTimer value object
        builder.Property(e => e.RestTimer)
            .HasConversion(
                timer => timer.Seconds,
                value => RestTimer.Create(value))
            .HasColumnName("RestTimer")
            .IsRequired();

        builder.Property(e => e.Day)
            .IsRequired();

        builder.Property(e => e.Order)
            .IsRequired();

        builder.Property(e => e.NumberOfSets)
            .IsRequired();

        // Map enums
        builder.Property(e => e.BodyCategory)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(e => e.EquipmentType)
            .HasConversion<string>()
            .IsRequired(false);

        builder.Property(e => e.WorkoutId)
            .IsRequired();

        // Configure relationship with progression
        builder.HasOne(e => e.Progression)
            .WithOne()
            .HasForeignKey<ExerciseProgression>(p => p.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

