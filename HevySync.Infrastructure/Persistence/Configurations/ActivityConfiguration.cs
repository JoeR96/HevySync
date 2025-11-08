using HevySync.Domain.Aggregates;
using HevySync.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HevySync.Infrastructure.Persistence.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.WorkoutId)
            .IsRequired();

        builder.Property(a => a.WorkoutName)
            .HasConversion(
                name => name.Value,
                value => WorkoutName.Create(value))
            .HasColumnName("WorkoutName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.StartedAt)
            .IsRequired();

        builder.Property(a => a.CompletedAt)
            .IsRequired(false);

        builder.Property(a => a.StoppedAt)
            .IsRequired(false);

        builder.HasIndex(a => new { a.UserId, a.Status });

        builder.Ignore(a => a.DomainEvents);
    }
}
