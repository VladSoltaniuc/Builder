// Configuration layer
namespace ProductApi.Configuration;

public sealed class WeeklyReportOptions
{
    // Master switch. Off by default; turned on in appsettings
    public bool Enabled { get; set; }

    // Day of week to send (0 = Sunday .. 6 = Saturday). Default Monday
    public int Day { get; set; } = 1;

    // Time of day (UTC) to send
    public int Hour { get; set; } = 8;
    public int Minute { get; set; }

    // Pass now in so Unit Tests can have stable output expectations
    public DateTime NextOccurrence(DateTime now)
    {
        var targetDow = Math.Clamp(Day, 0, 6);
        var daysAhead = (targetDow - (int)now.DayOfWeek + 7) % 7;
        var candidate = now.Date.AddDays(daysAhead).AddHours(Hour).AddMinutes(Minute);
        return candidate > now ? candidate : candidate.AddDays(7);
    }
}
