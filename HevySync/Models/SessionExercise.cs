using System.ComponentModel.DataAnnotations.Schema;
using HevySync.Models.Exercises;

namespace HevySync.Models;

public class SessionExercise
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public ICollection<Set> Sets { get; set; } = new List<Set>();
    public Exercise Exercise { get; set; }
    public Guid ExerciseId { get; set; }
}

public record SessionExerciseDto
{
    public Guid Id { get; set; }
    public ICollection<SetDto> SessionExercises { get; set; } = new List<SetDto>();
    public ExerciseDto Exercise { get; set; }
    public Guid ExerciseId { get; set; }
}

public static class SessionExerciseMapping
{
    public static SessionExerciseDto ToDto(this SessionExercise sessionExercise)
    {
        return new SessionExerciseDto
        {
            Id = sessionExercise.Id,
            ExerciseId = sessionExercise.ExerciseId,
            SessionExercises = sessionExercise.Sets.Select(set => set.ToDto()).ToList(),
            Exercise = sessionExercise.Exercise.ToDto()
        };
    }
}