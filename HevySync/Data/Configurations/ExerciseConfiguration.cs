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

        builder.Property(e => e.ExerciseTemplateId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.NumberOfSets)
            .IsRequired();

        builder.Property(e => e.RestTimer)
            .IsRequired();

        builder.Property(e => e.ExerciseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Day)
            .IsRequired();

        builder.Property(e => e.Order)
            .IsRequired();

        builder.Property(e => e.ExerciseProgram)
            .IsRequired();

        builder.Property(e => e.BodyCategory)
            .IsRequired();

        builder.Property(e => e.EquipmentType)
            .IsRequired();

        builder.Property(e => e.NumberOfSets)
            .IsRequired();

        builder.HasOne(e => e.ExerciseDetail)
            .WithOne(ed => ed.Exercise)
            .HasForeignKey<ExerciseDetail>(ed => ed.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}