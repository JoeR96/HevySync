using HevySync.Models;
using HevySync.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

internal class SessionExerciseConfiguration : IEntityTypeConfiguration<SessionExercise>
{
    public void Configure(EntityTypeBuilder<SessionExercise> builder)
    {
        builder.ToTable("SessionExercises");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Exercise)
            .WithMany()
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Sets)
            .WithOne()
            .HasForeignKey("SessionExerciseId")
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(s => s.ExerciseId);
    }
}