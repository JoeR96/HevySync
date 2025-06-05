using System.ComponentModel.DataAnnotations.Schema;
using HevySync.Models.Exercises;

namespace HevySync.Models;

public class Workout
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string Name { get; set; }
    public Guid ApplicationUserId { get; set; }
    public ICollection<Exercise> Exercises { get; set; }
}