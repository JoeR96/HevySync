using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExerciseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Day)
            .IsRequired();

        builder.Property(e => e.Method)
            .IsRequired();

        builder.Property(e => e.Category)
            .IsRequired();

        builder.Property(e => e.EquipmentType)
            .IsRequired();

        builder.HasOne(e => e.RepsPerSet)
            .WithOne(r => r.Exercise)
            .HasForeignKey<RepsPerSet>(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.LinearProgression)
            .WithOne(l => l.Exercise)
            .HasForeignKey<LinearProgression>(l => l.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}