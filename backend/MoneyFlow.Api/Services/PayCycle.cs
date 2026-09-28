namespace MoneyFlow.Api.Services;

public static class PayCycle
{
    public const int DefaultPaydayDay = 26;

    public static DateTime TodayInBangkok()
    {
        var tz = ResolveBangkok();
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
    }

    public static DateTime PaydayOnOrBefore(int year, int month, int paydayDay, bool skipWeekend)
    {
        var day = Math.Clamp(paydayDay, 1, DateTime.DaysInMonth(year, month));
        var date = new DateTime(year, month, day);
        if (!skipWeekend) return date;
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(-1),
            DayOfWeek.Sunday => date.AddDays(-2),
            _ => date
        };
    }

    public static (DateTime Start, DateTime End) PeriodRange(int year, int month, int paydayDay, bool skipWeekend)
    {
        var start = PaydayOnOrBefore(year, month, paydayDay, skipWeekend);
        var next = new DateTime(year, month, 1).AddMonths(1);
        var nextStart = PaydayOnOrBefore(next.Year, next.Month, paydayDay, skipWeekend);
        return (start, nextStart.AddDays(-1));
    }

    public static (int Year, int Month) PeriodContaining(DateTime today, int paydayDay, bool skipWeekend)
    {
        var date = today.Date;
        var thisStart = PaydayOnOrBefore(date.Year, date.Month, paydayDay, skipWeekend);
        if (date >= thisStart) return (date.Year, date.Month);
        var prev = new DateTime(date.Year, date.Month, 1).AddMonths(-1);
        return (prev.Year, prev.Month);
    }

    public static (int DayOfPeriod, int DaysInPeriod, int RemainingDays, double PercentElapsed, DateTime Start, DateTime End) Progress(DateTime today, int year, int month, int paydayDay, bool skipWeekend)
    {
        var (start, end) = PeriodRange(year, month, paydayDay, skipWeekend);
        var days = Math.Max(1, (end - start).Days + 1);
        var date = today.Date;
        int day;
        int remainingDays;
        if (date < start)
        {
            day = 1;
            remainingDays = days;
        }
        else if (date > end)
        {
            day = days;
            remainingDays = 0;
        }
        else
        {
            day = (date - start).Days + 1;
            remainingDays = (end - date).Days + 1;
        }
        var pct = Math.Round(day * 100.0 / days, 1);
        return (day, days, remainingDays, pct, start, end);
    }

    static TimeZoneInfo ResolveBangkok()
    {
        foreach (var id in new[] { "SE Asia Standard Time", "Asia/Bangkok" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }
}
