namespace PlantApp.Api;

public record CareStatus(DateTime NextWateringUtc, bool Overdue, int DaysUntilWatering);

public static class CareScheduler
{
    // Rain and cool weather stretch the interval; hot weather shortens it.
    public static int AdjustedInterval(int baseDays, double? temperatureC, double? rainMm)
    {
        var days = Math.Max(1, baseDays);
        if (temperatureC is >= 30) days = (int)Math.Max(1, Math.Round(days * 0.7));
        else if (temperatureC is <= 10) days = (int)Math.Round(days * 1.3);
        if (rainMm is >= 5) days += 1;
        return days;
    }

    public static CareStatus Evaluate(Plant plant, DateTime nowUtc, double? temperatureC = null, double? rainMm = null)
    {
        var interval = AdjustedInterval(plant.WateringIntervalDays, temperatureC, rainMm);
        var last = plant.LastWateredUtc ?? plant.CreatedAt;
        var next = last.AddDays(interval);
        var days = (int)Math.Ceiling((next - nowUtc).TotalDays);
        return new CareStatus(next, next <= nowUtc, days);
    }
}
