using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

public class ExerciseDetailConfiguration : IEntityTypeConfiguration<ExerciseDetail>
{
    public void Configure(EntityTypeBuilder<ExerciseDetail> builder)
    {
        builder.ToTable("ExerciseDetails");

        builder.HasKey(ed => ed.Id);

        builder.HasOne(ed => ed.Exercise)
            .WithOne(e => e.ExerciseDetail)
            .HasForeignKey<ExerciseDetail>(ed => ed.ExerciseId);
    }
}