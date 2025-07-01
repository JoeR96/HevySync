using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HevySync.Models.Exercises;

public class ExerciseInformation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public DateTime CompletedDate { get; set; }
    public int CompletedSets { get; set; }
    [MaxLength(255)] public string CompletedReps { get; set; }
    [Required] public Guid ExerciseId { get; set; }

    public virtual Exercise Exercise { get; set; }
    public decimal WorkingWeight { get; set; }
}