using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

public class LinearProgressionConfiguration : IEntityTypeConfiguration<LinearProgression>
{
    public void Configure(EntityTypeBuilder<LinearProgression> builder)
    {
        builder.ToTable("LinearProgressions");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.WeightProgression)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(l => l.AttemptsBeforeDeload)
            .IsRequired();
    }
}