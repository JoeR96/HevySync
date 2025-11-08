using HevySync.Domain.Common;
using HevySync.Domain.Enums;

namespace HevySync.Domain.Entities;

/// <summary>
/// Base class for exercise progression strategies.
/// Uses Table Per Hierarchy (TPH) pattern for persistence.
/// </summary>
public abstract class ExerciseProgression : Entity<Guid>
{
    public Guid ExerciseId { get; protected set; }
    public abstract ProgramType ProgramType { get; }

    protected ExerciseProgression(Guid id, Guid exerciseId) : base(id)
    {
        ExerciseId = exerciseId;
    }

    protected ExerciseProgression()
    {
    }
}

