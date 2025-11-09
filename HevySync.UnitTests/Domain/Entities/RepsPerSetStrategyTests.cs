using FluentAssertions;
using HevySync.Domain.Entities;
using HevySync.Domain.ValueObjects;
using NUnit.Framework;

namespace HevySync.UnitTests.Domain.Entities;

[TestFixture]
public class RepsPerSetStrategyTests
{
    [Test]
    public void Create_WithValidParameters_ShouldCreateStrategy()
    {
        var repRange = RepRange.Create(8, 10, 12);
        var weightProgression = WeightProgression.Create(2.5m);

        var strategy = RepsPerSetStrategy.Create(
            Guid.Empty,
            repRange,
            3, 5, 100m,
            weightProgression);

        strategy.Should().NotBeNull();
        strategy.RepRange.Should().Be(repRange);
        strategy.StartingSetCount.Should().Be(3);
        strategy.TargetSetCount.Should().Be(5);
        strategy.StartingWeight.Should().Be(100m);
        strategy.WeightProgression.Should().Be(weightProgression);
    }

    [Test]
    public void ApplyProgression_WithSuccess_ShouldIncreaseSets()
    {
        var strategy = CreateTestStrategy();
        var initialSets = strategy.StartingSetCount;

        strategy.ApplyProgression(PerformanceResult.Success);

        strategy.StartingSetCount.Should().Be(initialSets + 1);
        strategy.StartingWeight.Should().Be(100m);
    }

    [Test]
    public void ApplyProgression_WithSuccessAtMaxSets_ShouldIncreaseWeight()
    {
        var strategy = CreateTestStrategy();
        strategy.ApplyProgression(PerformanceResult.Success);
        strategy.ApplyProgression(PerformanceResult.Success);

        var initialWeight = strategy.StartingWeight;
        strategy.ApplyProgression(PerformanceResult.Success);

        strategy.StartingSetCount.Should().Be(5);
        strategy.StartingWeight.Should().Be(initialWeight + 2.5m);
    }

    [Test]
    public void ApplyProgression_WithFailure_ShouldDecreaseSets()
    {
        var strategy = CreateTestStrategy();
        strategy.ApplyProgression(PerformanceResult.Success);
        var initialSets = strategy.StartingSetCount;

        strategy.ApplyProgression(PerformanceResult.Failed);

        strategy.StartingSetCount.Should().Be(initialSets - 1);
    }

    [Test]
    public void ApplyProgression_WithFailureAtMinSets_ShouldDecreaseSetsFirst()
    {
        var strategy = CreateTestStrategy();

        strategy.ApplyProgression(PerformanceResult.Failed);

        strategy.StartingSetCount.Should().Be(2);
        strategy.StartingWeight.Should().Be(100m);
    }

    [Test]
    public void ApplyProgression_WithFailureAtMinWeight_ShouldNotGoNegative()
    {
        var strategy = RepsPerSetStrategy.Create(
            Guid.Empty,
            RepRange.Create(8, 10, 12),
            3, 5, 1m,
            WeightProgression.Create(2.5m));

        strategy.ApplyProgression(PerformanceResult.Failed);

        strategy.StartingWeight.Should().BeGreaterOrEqualTo(0m);
    }

    private static RepsPerSetStrategy CreateTestStrategy() =>
        RepsPerSetStrategy.Create(
            Guid.Empty,
            RepRange.Create(8, 10, 12),
            3, 5, 100m,
            WeightProgression.Create(2.5m));
}
