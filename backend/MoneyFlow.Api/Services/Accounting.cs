namespace MoneyFlow.Api.Services;

/// <summary>
/// Authoritative accounting formulas. BudgetAllocations.AllocatedAmount is planning-only and is NEVER added to TotalExpenses.
/// Remaining = TotalIncome - TotalExpenses - TotalSavings
/// TotalExpenses = Simple expense entries + actual transactions against expense-type budget categories.
/// </summary>
public static class Accounting
{
    public static decimal Remaining(decimal totalIncome, decimal totalExpenses, decimal totalSavings) =>
        totalIncome - totalExpenses - totalSavings;

    public static decimal TotalExpenses(decimal simpleExpenses, decimal transactionExpenses) =>
        simpleExpenses + transactionExpenses;

    public static decimal CategoryRemaining(decimal allocated, decimal used) => allocated - used;

    public static decimal DailyAllowance(decimal remaining, decimal todayUsed, int remainingDays)
    {
        if (remainingDays <= 0) return 0m;
        var leftoverIncludingToday = remaining + todayUsed;
        return leftoverIncludingToday > 0m ? leftoverIncludingToday / remainingDays : 0m;
    }

    public static bool OverDailyPace(decimal todayUsed, decimal dailyAllowance) => todayUsed > dailyAllowance;
}
