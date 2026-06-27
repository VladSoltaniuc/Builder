// Infrastructure layer — config for the index-reindex maintenance job
namespace ProductApi.Maintenance;

public enum MaintenanceFrequency
{
    Weekly,
    Monthly,
}

public sealed class IndexMaintenanceOptions
{
    // Master switch. Off by default; turned on in appsettings.
    public bool Enabled { get; set; }

    // Weekly or Monthly.
    public MaintenanceFrequency Frequency { get; set; } = MaintenanceFrequency.Weekly;

    // Weekly  -> day of week  (0 = Sunday .. 6 = Saturday)
    // Monthly -> day of month (1 .. 28; capped at 28 so it exists in every month)
    public int Day { get; set; }

    // Time of day to run.
    public int Hour { get; set; } = 3;
    public int Minute { get; set; }

    // An index is reindexed only when at least this % of it is dead/free space.
    // A freshly built btree is already ~10% free (fillfactor), so keep this well above that.
    public double BloatThresholdPercent { get; set; } = 30;

    // Ignore tiny indexes — their free % is noisy and rebuilding them buys nothing.
    public double MinIndexSizeMb { get; set; } = 1;

    // The next time the job should fire, strictly after 'now'. Pure + UTC so it's unit-testable.
    public DateTime NextOccurrence(DateTime now)
    {
        if (Frequency == MaintenanceFrequency.Weekly)
        {
            var targetDow = Math.Clamp(Day, 0, 6);
            var daysAhead = (targetDow - (int)now.DayOfWeek + 7) % 7;
            var candidate = now.Date.AddDays(daysAhead).AddHours(Hour).AddMinutes(Minute);
            return candidate > now ? candidate : candidate.AddDays(7);
        }

        // Monthly — Day is capped at 28 so the date is valid in every month.
        var dom = Math.Clamp(Day, 1, 28);
        var monthly = new DateTime(now.Year, now.Month, dom, Hour, Minute, 0, now.Kind);
        return monthly > now ? monthly : monthly.AddMonths(1);
    }
}
