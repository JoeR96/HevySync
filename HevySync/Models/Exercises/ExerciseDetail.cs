using System.ComponentModel.DataAnnotations.Schema;

namespace HevySync.Models.Exercises;

public abstract class ExerciseDetail
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
}