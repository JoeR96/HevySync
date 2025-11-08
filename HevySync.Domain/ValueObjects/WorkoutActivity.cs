using HevySync.Domain.Common;

namespace HevySync.Domain.ValueObjects;

public sealed class WorkoutActivity : ValueObject
{
    public int Week { get; }
    public int Day { get; }
    public int WorkoutsInWeek { get; }

    private WorkoutActivity(int week, int day, int workoutsInWeek)
    {
        Week = week;
        Day = day;
        WorkoutsInWeek = workoutsInWeek;
    }

    public static WorkoutActivity CreateInitial(int workoutsInWeek)
    {
        if (workoutsInWeek < 1 || workoutsInWeek > 7)
        {
            throw new ArgumentException("Workouts in week must be between 1 and 7", nameof(workoutsInWeek));
        }

        return new WorkoutActivity(1, 1, workoutsInWeek);
    }

    public static WorkoutActivity Create(int week, int day, int workoutsInWeek)
    {
        if (week < 1)
        {
            throw new ArgumentException("Week must be at least 1", nameof(week));
        }

        if (day < 1 || day > workoutsInWeek)
        {
            throw new ArgumentException($"Day must be between 1 and {workoutsInWeek}", nameof(day));
        }

        if (workoutsInWeek < 1 || workoutsInWeek > 7)
        {
            throw new ArgumentException("Workouts in week must be between 1 and 7", nameof(workoutsInWeek));
        }

        return new WorkoutActivity(week, day, workoutsInWeek);
    }

    public WorkoutActivity AdvanceDay()
    {
        if (Day < WorkoutsInWeek)
        {
            return new WorkoutActivity(Week, Day + 1, WorkoutsInWeek);
        }
        else
        {
            return new WorkoutActivity(Week + 1, 1, WorkoutsInWeek);
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Week;
        yield return Day;
        yield return WorkoutsInWeek;
    }

    public override string ToString() => $"Week {Week}, Day {Day} of {WorkoutsInWeek}";
}

