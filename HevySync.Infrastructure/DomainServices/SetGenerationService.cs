using HevySync.Domain.DomainServices;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;

namespace HevySync.Infrastructure.DomainServices;

public class SetGenerationService : ISetGenerationService
{
    private readonly ILinearProgressionCalculator _linearProgressionCalculator;
    private readonly IRepsPerSetCalculator _repsPerSetCalculator;

    public SetGenerationService(
        ILinearProgressionCalculator linearProgressionCalculator,
        IRepsPerSetCalculator repsPerSetCalculator)
    {
        _linearProgressionCalculator = linearProgressionCalculator;
        _repsPerSetCalculator = repsPerSetCalculator;
    }

    public async Task<List<Set>> GenerateWeekOneSetsAsync(
        ExerciseProgression progression,
        WorkoutActivity activity,
        CancellationToken cancellationToken = default)
    {
        return progression switch
        {
            LinearProgressionStrategy lp => await _linearProgressionCalculator.CalculateSetsAsync(lp, activity, cancellationToken),
            RepsPerSetStrategy rps => await _repsPerSetCalculator.CalculateSetsAsync(rps, cancellationToken),
            _ => throw new ArgumentException($"Unsupported progression type: {progression.GetType().Name}")
        };
    }
}

