using FluentAssertions;
using ProductApi.Maintenance;

namespace ProductApi.UnitTests.Maintenance;

public class IndexMaintenanceOptionsTests
{
    // 2026-06-29 is a Monday.
    private static readonly DateTime Monday0900 = new(2026, 6, 29, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Weekly_PicksNextMatchingWeekday_AtConfiguredTime()
    {
        var opts = new IndexMaintenanceOptions
        {
            Frequency = MaintenanceFrequency.Weekly,
            Day = 0, // Sunday
            Hour = 3,
            Minute = 30,
        };

        // From Monday, next Sunday 03:30 is 2026-07-05.
        opts.NextOccurrence(Monday0900)
            .Should().Be(new DateTime(2026, 7, 5, 3, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Weekly_WhenTodayIsTheDayButTimePassed_RollsToNextWeek()
    {
        var opts = new IndexMaintenanceOptions
        {
            Frequency = MaintenanceFrequency.Weekly,
            Day = 1, // Monday (== 'now')
            Hour = 3, // already past 09:00
        };

        opts.NextOccurrence(Monday0900)
            .Should().Be(new DateTime(2026, 7, 6, 3, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Monthly_WhenDayStillAhead_StaysInSameMonth()
    {
        var opts = new IndexMaintenanceOptions
        {
            Frequency = MaintenanceFrequency.Monthly,
            Day = 20,
            Hour = 3,
        };

        opts.NextOccurrence(new DateTime(2026, 6, 10, 9, 0, 0, DateTimeKind.Utc))
            .Should().Be(new DateTime(2026, 6, 20, 3, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Monthly_WhenDayAlreadyPassed_RollsToNextMonth()
    {
        var opts = new IndexMaintenanceOptions
        {
            Frequency = MaintenanceFrequency.Monthly,
            Day = 1,
            Hour = 3,
        };

        opts.NextOccurrence(Monday0900)
            .Should().Be(new DateTime(2026, 7, 1, 3, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Monthly_DayBeyond28_IsCappedTo28()
    {
        var opts = new IndexMaintenanceOptions
        {
            Frequency = MaintenanceFrequency.Monthly,
            Day = 31,
            Hour = 3,
        };

        // Capped to the 28th so the date is valid in every month.
        opts.NextOccurrence(new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc))
            .Should().Be(new DateTime(2026, 2, 28, 3, 0, 0, DateTimeKind.Utc));
    }
}
