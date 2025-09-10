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

public static class SetMappingExtensions
{
    public static SetDto ToDto(this Set set)
    {
        return new SetDto
        {
            WeightKg = set.WeightKg,
            Reps = set.Reps
        };
    }
}