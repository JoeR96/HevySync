using HevySync.Domain.Entities;
using HevySync.Domain.Enums;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class ExerciseProgressionConfiguration : IEntityTypeConfiguration<ExerciseProgression>
{
    public void Configure(EntityTypeBuilder<ExerciseProgression> builder)
    {
        builder.ToTable("ExerciseProgressions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever(); // We generate IDs in the domain

        builder.Property(p => p.ExerciseId)
            .IsRequired();

        // Ignore the ProgramType property (it's computed from the derived type)
        builder.Ignore(p => p.ProgramType);

        // Configure TPH (Table Per Hierarchy) discriminator using a shadow property
        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<LinearProgressionStrategy>("LinearProgression")
            .HasValue<RepsPerSetStrategy>("RepsPerSet");
    }
}

public class LinearProgressionStrategyConfiguration : IEntityTypeConfiguration<LinearProgressionStrategy>
{
    public void Configure(EntityTypeBuilder<LinearProgressionStrategy> builder)
    {
        // Map TrainingMax value object
        builder.Property(lp => lp.TrainingMax)
            .HasConversion(
                tm => tm.Value,
                value => TrainingMax.Create(value))
            .HasColumnName("TrainingMax")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Map WeightProgression value object
        builder.Property(lp => lp.WeightProgression)
            .HasConversion(
                wp => wp.Value,
                value => WeightProgression.Create(value))
            .HasColumnName("WeightProgression")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(lp => lp.AttemptsBeforeDeload)
            .IsRequired();

        builder.Property(lp => lp.IsPrimary)
            .HasColumnName("Primary")
            .IsRequired();
    }
}

public class RepsPerSetStrategyConfiguration : IEntityTypeConfiguration<RepsPerSetStrategy>
{
    public void Configure(EntityTypeBuilder<RepsPerSetStrategy> builder)
    {
        builder.OwnsOne(rps => rps.RepRange, repRange =>
        {
            repRange.Property(rr => rr.MinimumReps)
                .HasColumnName("MinimumReps")
                .IsRequired();

            repRange.Property(rr => rr.TargetReps)
                .HasColumnName("TargetReps")
                .IsRequired();

            repRange.Property(rr => rr.MaximumReps)
                .HasColumnName("MaximumTargetReps")
                .IsRequired();
        });

        builder.Property(rps => rps.StartingSetCount)
            .IsRequired();

        builder.Property(rps => rps.TargetSetCount)
            .IsRequired();

        builder.Property(rps => rps.StartingWeight)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(rps => rps.WeightProgression)
            .HasConversion(
                wp => wp.Value,
                value => WeightProgression.Create(value))
            .HasColumnName("RepsPerSetWeightProgression")
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}

