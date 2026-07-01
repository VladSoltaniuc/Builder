// Configuration layer
namespace ProductApi.Configuration;

public sealed class IndexMaintenanceOptions
{
    public bool Enabled { get; set; }
    public MaintenanceFrequency Frequency { get; set; } = MaintenanceFrequency.Weekly;

    // Weekly  -> day of week  (0 = Sunday .. 6 = Saturday)
    // Monthly -> day of month (1 .. 28; capped at 28 so it exists in every month)
    public int Day { get; set; }

    public int Hour { get; set; } = 3;
    public int Minute { get; set; }

    // Dead % that triggers reindexing (fresh btree is 10%, keep it above that)
    public double BloatThresholdPercent { get; set; } = 30;

    // Ignore tiny indexes, rebuilding them buys nothing
    public double MinIndexSizeMb { get; set; } = 1;

    // UTC based maintenance
    public DateTime NextOccurrence(DateTime now)
    {
        if (Frequency == MaintenanceFrequency.Weekly)
        {
            var targetDow = Math.Clamp(Day, 0, 6);
            var daysAhead = (targetDow - (int)now.DayOfWeek + 7) % 7;
            var candidate = now.Date.AddDays(daysAhead).AddHours(Hour).AddMinutes(Minute);
            return candidate > now ? candidate : candidate.AddDays(7);
        }

        // Monthly, Day is capped at 28 so the date is valid in every month
        var dom = Math.Clamp(Day, 1, 28);
        var monthly = new DateTime(now.Year, now.Month, dom, Hour, Minute, 0, now.Kind);
        return monthly > now ? monthly : monthly.AddMonths(1);
    }
}

public enum MaintenanceFrequency
{
    Weekly  = 0,
    Monthly = 1,
}
