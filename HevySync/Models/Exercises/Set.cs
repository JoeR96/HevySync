using System.ComponentModel.DataAnnotations.Schema;

namespace HevySync.Models.Exercises;

public class Set
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
}

public record SetDto
{
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
}