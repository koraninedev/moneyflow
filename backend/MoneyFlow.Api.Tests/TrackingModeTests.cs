using MoneyFlow.Api.Common;
using MoneyFlow.Api.Dtos;
using MoneyFlow.Api.Models;
using MoneyFlow.Api.Repositories;
using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Tests;

public class TrackingModeTests
{
    [Fact]
    public async Task ExpenseEntry_Rejected_WhenCategoryIsBudgetVsActual()
    {
        var categories = new StubCategories(new Category { CategoryId = 6, UserId = 1, Type = CategoryType.Expense, TrackingMode = TrackingMode.BudgetVsActual, Name = "Food" });
        var periods = new StubPeriods(new MonthlyPeriod { MonthlyPeriodId = 1, UserId = 1, Year = 2026, Month = 9 });
        var svc = new ExpensesService(new StubExpenses(), periods, categories);
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            svc.CreateAsync(1, new CreateExpenseRequest(1, 6, "Groceries", 100, Classification.Variable, null, null)));
        Assert.Contains("Simple", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Transaction_Rejected_WhenCategoryIsSimple()
    {
        var categories = new StubCategories(new Category { CategoryId = 3, UserId = 1, Type = CategoryType.Expense, TrackingMode = TrackingMode.Simple, Name = "Rent" });
        var periods = new StubPeriods(new MonthlyPeriod { MonthlyPeriodId = 1, UserId = 1, Year = 2026, Month = 9 });
        var svc = new TransactionsService(new StubTransactions(), periods, categories);
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            svc.CreateAsync(1, new CreateTransactionRequest(1, 3, 100, DateTime.UtcNow, "oops", null)));
        Assert.Contains("BudgetVsActual", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Transaction_Rejected_WhenDateIsFuture()
    {
        var categories = new StubCategories(new Category { CategoryId = 6, UserId = 1, Type = CategoryType.Expense, TrackingMode = TrackingMode.BudgetVsActual, Name = "Food" });
        var periods = new StubPeriods(new MonthlyPeriod { MonthlyPeriodId = 1, UserId = 1, Year = 2026, Month = 9 });
        var svc = new TransactionsService(new StubTransactions(), periods, categories);
        var future = PayCycle.TodayInBangkok().AddDays(1);
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            svc.CreateAsync(1, new CreateTransactionRequest(1, 6, 100, future, "later", null)));
        Assert.Contains("future", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

file sealed class StubCategories(Category category) : ICategoriesRepository
{
    public Task<IEnumerable<Category>> GetAllAsync(int userId, string? type) => Task.FromResult<IEnumerable<Category>>([category]);
    public Task<Category?> GetByIdAsync(int userId, int categoryId) => Task.FromResult<Category?>(category.CategoryId == categoryId ? category : null);
    public Task<int> InsertAsync(Category c) => Task.FromResult(1);
    public Task<bool> UpdateAsync(Category c) => Task.FromResult(true);
    public Task<bool> DeactivateAsync(int userId, int categoryId) => Task.FromResult(true);
    public Task SeedDefaultsForUserAsync(int userId) => Task.CompletedTask;
}

file sealed class StubPeriods(MonthlyPeriod period) : IMonthlyPeriodsRepository
{
    public Task<IEnumerable<MonthlyPeriod>> GetAllAsync(int userId) => Task.FromResult<IEnumerable<MonthlyPeriod>>([period]);
    public Task<MonthlyPeriod?> GetByIdAsync(int userId, int monthlyPeriodId) => Task.FromResult<MonthlyPeriod?>(period.MonthlyPeriodId == monthlyPeriodId ? period : null);
    public Task<MonthlyPeriod?> GetByYearMonthAsync(int userId, int year, int month) => Task.FromResult<MonthlyPeriod?>(period);
    public Task<bool> ExistsAsync(int userId, int year, int month) => Task.FromResult(false);
    public Task<int> InsertAsync(int userId, int year, int month) => Task.FromResult(period.MonthlyPeriodId);
}

file sealed class StubExpenses : IExpenseEntriesRepository
{
    public Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId) => Task.FromResult<IEnumerable<dynamic>>([]);
    public Task<ExpenseEntry?> GetByIdAsync(int userId, int expenseEntryId) => Task.FromResult<ExpenseEntry?>(null);
    public Task<int> InsertAsync(ExpenseEntry entry) => Task.FromResult(1);
    public Task<bool> UpdateAsync(ExpenseEntry entry) => Task.FromResult(true);
    public Task<bool> SetPaidAsync(int userId, int expenseEntryId, bool isPaid, DateTime? paidDate) => Task.FromResult(true);
    public Task<bool> DeleteAsync(int userId, int expenseEntryId) => Task.FromResult(true);
    public Task ReorderAsync(int userId, int monthlyPeriodId, IReadOnlyList<int> orderedIds) => Task.CompletedTask;
}

file sealed class StubTransactions : ITransactionsRepository
{
    public Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, int? categoryId) => Task.FromResult<IEnumerable<dynamic>>([]);
    public Task<TransactionEntry?> GetByIdAsync(int userId, int transactionId) => Task.FromResult<TransactionEntry?>(null);
    public Task<int> InsertAsync(TransactionEntry entry) => Task.FromResult(1);
    public Task<bool> UpdateAsync(TransactionEntry entry) => Task.FromResult(true);
    public Task<bool> DeleteAsync(int userId, int transactionId) => Task.FromResult(true);
}
