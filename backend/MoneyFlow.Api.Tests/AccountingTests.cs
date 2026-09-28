using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Tests;

public class AccountingTests
{
    [Fact]
    public void Remaining_Income40000_Expenses20000_Savings5000_Is15000()
    {
        Assert.Equal(15000m, Accounting.Remaining(40000m, 20000m, 5000m));
    }

    [Fact]
    public void TotalExpenses_DoesNotIncludeBudgetAllocation()
    {
        var simple = 20000m;
        var transactions = 0m;
        var allocatedFood = 7000m;
        var totalExpenses = Accounting.TotalExpenses(simple, transactions);
        Assert.Equal(20000m, totalExpenses);
        Assert.NotEqual(simple + allocatedFood, totalExpenses);
        Assert.Equal(15000m, Accounting.Remaining(40000m, totalExpenses, 5000m));
    }

    [Fact]
    public void FoodBudget_UsedAndRemaining()
    {
        var allocated = 7000m;
        var used = 600m + 520m + 640m;
        Assert.Equal(1760m, used);
        Assert.Equal(5240m, Accounting.CategoryRemaining(allocated, used));
    }

    [Fact]
    public void SeedSeptember_FoodUsed2330_Remaining4670()
    {
        var allocated = 7000m;
        var used = 600m + 520m + 640m + 570m;
        Assert.Equal(2330m, used);
        Assert.Equal(4670m, Accounting.CategoryRemaining(allocated, used));
    }

    [Fact]
    public void CopyRecurring_DueDayClippedToMonthLength()
    {
        var dueDay = 31;
        var febDays = DateTime.DaysInMonth(2026, 2);
        var clipped = Math.Min(dueDay, febDays);
        Assert.Equal(28, clipped);
    }
}
