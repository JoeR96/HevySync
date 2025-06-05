using HevySync.Models.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Data.Configurations;

public class RepsPerSetConfiguration : IEntityTypeConfiguration<RepsPerSet>
{
    public void Configure(EntityTypeBuilder<RepsPerSet> builder)
    {
        builder.ToTable("RepsPerSets");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.MinimumReps).IsRequired();
        builder.Property(r => r.TargetReps).IsRequired();
        builder.Property(r => r.MaximumTargetReps).IsRequired();
        builder.Property(r => r.NumberOfSets).IsRequired();
        builder.Property(r => r.TotalNumberOfSets).IsRequired();
    }
}