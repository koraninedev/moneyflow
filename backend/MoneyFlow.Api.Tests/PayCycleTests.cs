using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Tests;

public class PayCycleTests
{
    [Fact]
    public void SaturdayPayday_MovesToFriday()
    {
        var payday = PayCycle.PaydayOnOrBefore(2026, 9, 26, skipWeekend: true);
        Assert.Equal(new DateTime(2026, 9, 25), payday);
        Assert.Equal(DayOfWeek.Friday, payday.DayOfWeek);
    }

    [Fact]
    public void SundayPayday_MovesToFriday()
    {
        Assert.Equal(DayOfWeek.Sunday, new DateTime(2026, 4, 26).DayOfWeek);
        var payday = PayCycle.PaydayOnOrBefore(2026, 4, 26, skipWeekend: true);
        Assert.Equal(new DateTime(2026, 4, 24), payday);
        Assert.Equal(DayOfWeek.Friday, payday.DayOfWeek);
    }

    [Fact]
    public void WeekdayPayday_Unchanged()
    {
        Assert.Equal(DayOfWeek.Monday, new DateTime(2026, 10, 26).DayOfWeek);
        Assert.Equal(new DateTime(2026, 10, 26), PayCycle.PaydayOnOrBefore(2026, 10, 26, skipWeekend: true));
    }

    [Fact]
    public void SkipWeekendOff_KeepsSaturday()
    {
        Assert.Equal(new DateTime(2026, 9, 26), PayCycle.PaydayOnOrBefore(2026, 9, 26, skipWeekend: false));
    }

    [Fact]
    public void September2026_PeriodStartsFriday25()
    {
        var (start, end) = PayCycle.PeriodRange(2026, 9, 26, skipWeekend: true);
        Assert.Equal(new DateTime(2026, 9, 25), start);
        Assert.Equal(new DateTime(2026, 10, 25), end);
    }

    [Fact]
    public void Sep27_IsDay3_OfSeptemberPeriod()
    {
        var p = PayCycle.Progress(new DateTime(2026, 9, 27), 2026, 9, 26, skipWeekend: true);
        Assert.Equal(3, p.DayOfPeriod);
        Assert.Equal(31, p.DaysInPeriod);
        Assert.Equal(29, p.RemainingDays);
    }

    [Fact]
    public void Oct1_StillSeptemberPeriod()
    {
        var period = PayCycle.PeriodContaining(new DateTime(2026, 10, 1), 26, skipWeekend: true);
        Assert.Equal(2026, period.Year);
        Assert.Equal(9, period.Month);
    }

    [Fact]
    public void Oct26_StartsOctoberPeriod()
    {
        var period = PayCycle.PeriodContaining(new DateTime(2026, 10, 26), 26, skipWeekend: true);
        Assert.Equal(2026, period.Year);
        Assert.Equal(10, period.Month);
    }

    [Fact]
    public void CalendarMonth_Payday1_MatchesCalendar()
    {
        var (start, end) = PayCycle.PeriodRange(2026, 9, 1, skipWeekend: false);
        Assert.Equal(new DateTime(2026, 9, 1), start);
        Assert.Equal(new DateTime(2026, 9, 30), end);
        var p = PayCycle.Progress(new DateTime(2026, 9, 27), 2026, 9, 1, skipWeekend: false);
        Assert.Equal(27, p.DayOfPeriod);
        Assert.Equal(30, p.DaysInPeriod);
        Assert.Equal(4, p.RemainingDays);
    }

    [Fact]
    public void PastPeriod_RemainingDaysZero()
    {
        var p = PayCycle.Progress(new DateTime(2026, 11, 1), 2026, 9, 26, skipWeekend: true);
        Assert.Equal(0, p.RemainingDays);
        Assert.Equal(p.DaysInPeriod, p.DayOfPeriod);
    }
}
