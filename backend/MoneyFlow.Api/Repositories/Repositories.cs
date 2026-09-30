using System.Data;
using Dapper;
using MoneyFlow.Api.Data;
using MoneyFlow.Api.Models;

namespace MoneyFlow.Api.Repositories;

public interface IUsersRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<int> InsertAsync(User user);
    Task<bool> UpdateSettingsAsync(int userId, int periodStartDay, bool skipWeekendPayday);
}

public sealed class UsersRepository(ISqlConnectionFactory factory) : IUsersRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<User>("SELECT UserId, Email, PasswordHash, DisplayName, IsActive, PeriodStartDay, SkipWeekendPayday, CreatedAt, UpdatedAt FROM Users WHERE Email = @Email", new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<User>("SELECT UserId, Email, PasswordHash, DisplayName, IsActive, PeriodStartDay, SkipWeekendPayday, CreatedAt, UpdatedAt FROM Users WHERE UserId = @UserId", new { UserId = userId });
    }

    public async Task<int> InsertAsync(User user)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO Users (Email, PasswordHash, DisplayName, PeriodStartDay, SkipWeekendPayday) OUTPUT INSERTED.UserId VALUES (@Email, @PasswordHash, @DisplayName, @PeriodStartDay, @SkipWeekendPayday)", user);
    }

    public async Task<bool> UpdateSettingsAsync(int userId, int periodStartDay, bool skipWeekendPayday)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE Users SET PeriodStartDay = @PeriodStartDay, SkipWeekendPayday = @SkipWeekendPayday, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId", new { UserId = userId, PeriodStartDay = periodStartDay, SkipWeekendPayday = skipWeekendPayday }) > 0;
    }
}

public interface ICategoriesRepository
{
    Task<IEnumerable<Category>> GetAllAsync(int userId, string? type);
    Task<Category?> GetByIdAsync(int userId, int categoryId);
    Task<int> InsertAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeactivateAsync(int userId, int categoryId);
    Task SeedDefaultsForUserAsync(int userId);
}

public sealed class CategoriesRepository(ISqlConnectionFactory factory) : ICategoriesRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync(int userId, string? type)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync<Category>("SELECT CategoryId, UserId, Name, Type, Classification, TrackingMode, Icon, Color, SortOrder, IsActive, CreatedAt FROM Categories WHERE UserId = @UserId AND IsActive = 1 AND (@Type IS NULL OR Type = @Type) ORDER BY Type, SortOrder, Name", new { UserId = userId, Type = type });
    }

    public async Task<Category?> GetByIdAsync(int userId, int categoryId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<Category>("SELECT CategoryId, UserId, Name, Type, Classification, TrackingMode, Icon, Color, SortOrder, IsActive, CreatedAt FROM Categories WHERE UserId = @UserId AND CategoryId = @CategoryId", new { UserId = userId, CategoryId = categoryId });
    }

    public async Task<int> InsertAsync(Category category)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO Categories (UserId, Name, Type, Classification, TrackingMode, Icon, Color, SortOrder) OUTPUT INSERTED.CategoryId VALUES (@UserId, @Name, @Type, @Classification, @TrackingMode, @Icon, @Color, @SortOrder)", category);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE Categories SET Name = @Name, Classification = @Classification, TrackingMode = @TrackingMode, Icon = @Icon, Color = @Color, SortOrder = @SortOrder WHERE UserId = @UserId AND CategoryId = @CategoryId", category) > 0;
    }

    public async Task<bool> DeactivateAsync(int userId, int categoryId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE Categories SET IsActive = 0 WHERE UserId = @UserId AND CategoryId = @CategoryId", new { UserId = userId, CategoryId = categoryId }) > 0;
    }

    public async Task SeedDefaultsForUserAsync(int userId)
    {
        using var c = factory.CreateConnection();
        await c.ExecuteAsync("""
            INSERT INTO Categories (UserId, Name, Type, Classification, TrackingMode, Color, SortOrder) VALUES
                (@UserId, N'Salary', 'Income', NULL, 'Simple', '#2F6FED', 1),
                (@UserId, N'Other Income', 'Income', NULL, 'Simple', '#2F6FED', 2),
                (@UserId, N'Rent', 'Expense', 'Variable', 'Simple', '#DC2626', 1),
                (@UserId, N'Utilities', 'Expense', 'Fixed', 'Simple', '#DC2626', 2),
                (@UserId, N'Credit Card', 'Expense', 'Fixed', 'Simple', '#DC2626', 3),
                (@UserId, N'Food', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 4),
                (@UserId, N'Reward', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 5),
                (@UserId, N'Transportation', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 6),
                (@UserId, N'Shopping', 'Expense', 'Variable', 'BudgetVsActual', '#D97706', 7),
                (@UserId, N'General Savings', 'Saving', NULL, 'Simple', '#16A34A', 1)
            """, new { UserId = userId });
    }
}

public interface IMonthlyPeriodsRepository
{
    Task<IEnumerable<MonthlyPeriod>> GetAllAsync(int userId);
    Task<MonthlyPeriod?> GetByIdAsync(int userId, int monthlyPeriodId);
    Task<MonthlyPeriod?> GetByYearMonthAsync(int userId, int year, int month);
    Task<bool> ExistsAsync(int userId, int year, int month);
    Task<int> InsertAsync(int userId, int year, int month);
}

public sealed class MonthlyPeriodsRepository(ISqlConnectionFactory factory) : IMonthlyPeriodsRepository
{
    public async Task<IEnumerable<MonthlyPeriod>> GetAllAsync(int userId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync<MonthlyPeriod>("SELECT MonthlyPeriodId, UserId, Year, Month, CreatedAt FROM MonthlyPeriods WHERE UserId = @UserId ORDER BY Year DESC, Month DESC", new { UserId = userId });
    }

    public async Task<MonthlyPeriod?> GetByIdAsync(int userId, int monthlyPeriodId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<MonthlyPeriod>("SELECT MonthlyPeriodId, UserId, Year, Month, CreatedAt FROM MonthlyPeriods WHERE UserId = @UserId AND MonthlyPeriodId = @MonthlyPeriodId", new { UserId = userId, MonthlyPeriodId = monthlyPeriodId });
    }

    public async Task<MonthlyPeriod?> GetByYearMonthAsync(int userId, int year, int month)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<MonthlyPeriod>("SELECT MonthlyPeriodId, UserId, Year, Month, CreatedAt FROM MonthlyPeriods WHERE UserId = @UserId AND Year = @Year AND Month = @Month", new { UserId = userId, Year = year, Month = month });
    }

    public async Task<bool> ExistsAsync(int userId, int year, int month)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM MonthlyPeriods WHERE UserId = @UserId AND Year = @Year AND Month = @Month", new { UserId = userId, Year = year, Month = month }) > 0;
    }

    public async Task<int> InsertAsync(int userId, int year, int month)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO MonthlyPeriods (UserId, Year, Month) OUTPUT INSERTED.MonthlyPeriodId VALUES (@UserId, @Year, @Month)", new { UserId = userId, Year = year, Month = month });
    }
}

public interface IIncomeEntriesRepository
{
    Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId);
    Task<IncomeEntry?> GetByIdAsync(int userId, int incomeEntryId);
    Task<int> InsertAsync(IncomeEntry entry);
    Task<bool> UpdateAsync(IncomeEntry entry);
    Task<bool> SetActiveAsync(int userId, int incomeEntryId, bool isActive);
    Task<bool> DeleteAsync(int userId, int incomeEntryId);
}

public sealed class IncomeEntriesRepository(ISqlConnectionFactory factory) : IIncomeEntriesRepository
{
    public async Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT ie.IncomeEntryId, ie.MonthlyPeriodId, ie.CategoryId, c.Name AS CategoryName, ie.Name, ie.Amount, ie.IncomeDate, ie.IsRecurring, ie.Note, ie.IsActive
            FROM IncomeEntries ie INNER JOIN Categories c ON c.CategoryId = ie.CategoryId
            WHERE ie.UserId = @UserId AND ie.MonthlyPeriodId = @MonthlyPeriodId
            ORDER BY ie.IncomeDate DESC, ie.IncomeEntryId DESC
            """, new { UserId = userId, MonthlyPeriodId = monthlyPeriodId });
    }

    public async Task<IncomeEntry?> GetByIdAsync(int userId, int incomeEntryId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<IncomeEntry>("SELECT IncomeEntryId, UserId, MonthlyPeriodId, CategoryId, Name, Amount, IncomeDate, IsRecurring, RecurringTemplateId, Note, IsActive, CreatedAt, UpdatedAt FROM IncomeEntries WHERE UserId = @UserId AND IncomeEntryId = @IncomeEntryId", new { UserId = userId, IncomeEntryId = incomeEntryId });
    }

    public async Task<int> InsertAsync(IncomeEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO IncomeEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, IncomeDate, IsRecurring, RecurringTemplateId, Note) OUTPUT INSERTED.IncomeEntryId VALUES (@UserId, @MonthlyPeriodId, @CategoryId, @Name, @Amount, @IncomeDate, @IsRecurring, @RecurringTemplateId, @Note)", entry);
    }

    public async Task<bool> UpdateAsync(IncomeEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE IncomeEntries SET CategoryId = @CategoryId, Name = @Name, Amount = @Amount, IncomeDate = @IncomeDate, IsRecurring = @IsRecurring, Note = @Note, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND IncomeEntryId = @IncomeEntryId", entry) > 0;
    }

    public async Task<bool> SetActiveAsync(int userId, int incomeEntryId, bool isActive)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE IncomeEntries SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND IncomeEntryId = @IncomeEntryId", new { UserId = userId, IncomeEntryId = incomeEntryId, IsActive = isActive }) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int incomeEntryId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("DELETE FROM IncomeEntries WHERE UserId = @UserId AND IncomeEntryId = @IncomeEntryId", new { UserId = userId, IncomeEntryId = incomeEntryId }) > 0;
    }
}

public interface IExpenseEntriesRepository
{
    Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId);
    Task<ExpenseEntry?> GetByIdAsync(int userId, int expenseEntryId);
    Task<int> InsertAsync(ExpenseEntry entry);
    Task<bool> UpdateAsync(ExpenseEntry entry);
    Task<bool> SetPaidAsync(int userId, int expenseEntryId, bool isPaid, DateTime? paidDate);
    Task<bool> DeleteAsync(int userId, int expenseEntryId);
    Task ReorderAsync(int userId, int monthlyPeriodId, IReadOnlyList<int> orderedIds);
}

public sealed class ExpenseEntriesRepository(ISqlConnectionFactory factory) : IExpenseEntriesRepository
{
    public async Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT ee.ExpenseEntryId, ee.MonthlyPeriodId, ee.CategoryId, c.Name AS CategoryName, ee.Name, ee.Amount, ee.Classification, ee.DueDate, ee.IsPaid, ee.PaidDate, ee.IsRecurring, ee.Note, ee.SortOrder
            FROM ExpenseEntries ee INNER JOIN Categories c ON c.CategoryId = ee.CategoryId
            WHERE ee.UserId = @UserId AND ee.MonthlyPeriodId = @MonthlyPeriodId
            ORDER BY ee.SortOrder, ee.ExpenseEntryId
            """, new { UserId = userId, MonthlyPeriodId = monthlyPeriodId });
    }

    public async Task<ExpenseEntry?> GetByIdAsync(int userId, int expenseEntryId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<ExpenseEntry>("SELECT ExpenseEntryId, UserId, MonthlyPeriodId, CategoryId, Name, Amount, Classification, DueDate, IsPaid, PaidDate, IsRecurring, RecurringTemplateId, Note, SortOrder, CreatedAt, UpdatedAt FROM ExpenseEntries WHERE UserId = @UserId AND ExpenseEntryId = @ExpenseEntryId", new { UserId = userId, ExpenseEntryId = expenseEntryId });
    }

    public async Task<int> InsertAsync(ExpenseEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO ExpenseEntries (UserId, MonthlyPeriodId, CategoryId, Name, Amount, Classification, DueDate, IsRecurring, RecurringTemplateId, Note, SortOrder) OUTPUT INSERTED.ExpenseEntryId VALUES (@UserId, @MonthlyPeriodId, @CategoryId, @Name, @Amount, @Classification, @DueDate, @IsRecurring, @RecurringTemplateId, @Note, @SortOrder)", entry);
    }

    public async Task<bool> UpdateAsync(ExpenseEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE ExpenseEntries SET CategoryId = @CategoryId, Name = @Name, Amount = @Amount, Classification = @Classification, DueDate = @DueDate, Note = @Note, SortOrder = @SortOrder, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND ExpenseEntryId = @ExpenseEntryId", entry) > 0;
    }

    public async Task<bool> SetPaidAsync(int userId, int expenseEntryId, bool isPaid, DateTime? paidDate)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE ExpenseEntries SET IsPaid = @IsPaid, PaidDate = @PaidDate, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND ExpenseEntryId = @ExpenseEntryId", new { UserId = userId, ExpenseEntryId = expenseEntryId, IsPaid = isPaid, PaidDate = paidDate }) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int expenseEntryId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("DELETE FROM ExpenseEntries WHERE UserId = @UserId AND ExpenseEntryId = @ExpenseEntryId", new { UserId = userId, ExpenseEntryId = expenseEntryId }) > 0;
    }

    public async Task ReorderAsync(int userId, int monthlyPeriodId, IReadOnlyList<int> orderedIds)
    {
        using var c = factory.CreateConnection();
        c.Open();
        using var tx = c.BeginTransaction();
        for (var i = 0; i < orderedIds.Count; i++)
        {
            await c.ExecuteAsync("UPDATE ExpenseEntries SET SortOrder = @SortOrder, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND MonthlyPeriodId = @MonthlyPeriodId AND ExpenseEntryId = @Id", new { SortOrder = i, UserId = userId, MonthlyPeriodId = monthlyPeriodId, Id = orderedIds[i] }, tx);
        }
        tx.Commit();
    }
}

public interface IBudgetAllocationsRepository
{
    Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, DateTime today);
    Task<BudgetAllocation?> GetByIdAsync(int userId, int budgetAllocationId);
    Task<BudgetAllocation?> GetByPeriodAndCategoryAsync(int userId, int monthlyPeriodId, int categoryId);
    Task<int> InsertAsync(BudgetAllocation allocation);
    Task<bool> UpdateAsync(BudgetAllocation allocation);
    Task<bool> DeleteAsync(int userId, int budgetAllocationId);
}

public sealed class BudgetAllocationsRepository(ISqlConnectionFactory factory) : IBudgetAllocationsRepository
{
    public async Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, DateTime today)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT ba.BudgetAllocationId, ba.MonthlyPeriodId, ba.CategoryId, c.Name AS CategoryName, ba.AllocatedAmount, ba.Note,
                   ISNULL((SELECT SUM(t.Amount) FROM Transactions t WHERE t.MonthlyPeriodId = ba.MonthlyPeriodId AND t.CategoryId = ba.CategoryId), 0) AS Used,
                   ISNULL((SELECT SUM(t.Amount) FROM Transactions t WHERE t.MonthlyPeriodId = ba.MonthlyPeriodId AND t.CategoryId = ba.CategoryId AND t.TransactionDate = @Today), 0) AS TodayUsed
            FROM BudgetAllocations ba INNER JOIN Categories c ON c.CategoryId = ba.CategoryId
            WHERE ba.UserId = @UserId AND ba.MonthlyPeriodId = @MonthlyPeriodId
            ORDER BY c.SortOrder, c.Name
            """, new { UserId = userId, MonthlyPeriodId = monthlyPeriodId, Today = today.Date });
    }

    public async Task<BudgetAllocation?> GetByIdAsync(int userId, int budgetAllocationId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<BudgetAllocation>("SELECT BudgetAllocationId, UserId, MonthlyPeriodId, CategoryId, AllocatedAmount, IsRecurring, RecurringTemplateId, Note, CreatedAt, UpdatedAt FROM BudgetAllocations WHERE UserId = @UserId AND BudgetAllocationId = @BudgetAllocationId", new { UserId = userId, BudgetAllocationId = budgetAllocationId });
    }

    public async Task<BudgetAllocation?> GetByPeriodAndCategoryAsync(int userId, int monthlyPeriodId, int categoryId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<BudgetAllocation>("SELECT BudgetAllocationId, UserId, MonthlyPeriodId, CategoryId, AllocatedAmount, IsRecurring, RecurringTemplateId, Note, CreatedAt, UpdatedAt FROM BudgetAllocations WHERE UserId = @UserId AND MonthlyPeriodId = @MonthlyPeriodId AND CategoryId = @CategoryId", new { UserId = userId, MonthlyPeriodId = monthlyPeriodId, CategoryId = categoryId });
    }

    public async Task<int> InsertAsync(BudgetAllocation allocation)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO BudgetAllocations (UserId, MonthlyPeriodId, CategoryId, AllocatedAmount, IsRecurring, RecurringTemplateId, Note) OUTPUT INSERTED.BudgetAllocationId VALUES (@UserId, @MonthlyPeriodId, @CategoryId, @AllocatedAmount, @IsRecurring, @RecurringTemplateId, @Note)", allocation);
    }

    public async Task<bool> UpdateAsync(BudgetAllocation allocation)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE BudgetAllocations SET AllocatedAmount = @AllocatedAmount, Note = @Note, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND BudgetAllocationId = @BudgetAllocationId", allocation) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int budgetAllocationId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("DELETE FROM BudgetAllocations WHERE UserId = @UserId AND BudgetAllocationId = @BudgetAllocationId", new { UserId = userId, BudgetAllocationId = budgetAllocationId }) > 0;
    }
}

public interface ITransactionsRepository
{
    Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, int? categoryId);
    Task<TransactionEntry?> GetByIdAsync(int userId, int transactionId);
    Task<int> InsertAsync(TransactionEntry entry);
    Task<bool> UpdateAsync(TransactionEntry entry);
    Task<bool> DeleteAsync(int userId, int transactionId);
}

public sealed class TransactionsRepository(ISqlConnectionFactory factory) : ITransactionsRepository
{
    public async Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, int? categoryId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT t.TransactionId, t.MonthlyPeriodId, t.CategoryId, c.Name AS CategoryName, t.Amount, t.TransactionDate, t.Description, t.Note
            FROM Transactions t INNER JOIN Categories c ON c.CategoryId = t.CategoryId
            WHERE t.UserId = @UserId AND t.MonthlyPeriodId = @MonthlyPeriodId AND (@CategoryId IS NULL OR t.CategoryId = @CategoryId)
            ORDER BY t.TransactionDate DESC, t.TransactionId DESC
            """, new { UserId = userId, MonthlyPeriodId = monthlyPeriodId, CategoryId = categoryId });
    }

    public async Task<TransactionEntry?> GetByIdAsync(int userId, int transactionId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<TransactionEntry>("SELECT TransactionId, UserId, MonthlyPeriodId, CategoryId, Amount, TransactionDate, Description, Note, CreatedAt, UpdatedAt FROM Transactions WHERE UserId = @UserId AND TransactionId = @TransactionId", new { UserId = userId, TransactionId = transactionId });
    }

    public async Task<int> InsertAsync(TransactionEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO Transactions (UserId, MonthlyPeriodId, CategoryId, Amount, TransactionDate, Description, Note) OUTPUT INSERTED.TransactionId VALUES (@UserId, @MonthlyPeriodId, @CategoryId, @Amount, @TransactionDate, @Description, @Note)", entry);
    }

    public async Task<bool> UpdateAsync(TransactionEntry entry)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE Transactions SET CategoryId = @CategoryId, Amount = @Amount, TransactionDate = @TransactionDate, Description = @Description, Note = @Note, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND TransactionId = @TransactionId", entry) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int transactionId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("DELETE FROM Transactions WHERE UserId = @UserId AND TransactionId = @TransactionId", new { UserId = userId, TransactionId = transactionId }) > 0;
    }
}

public interface ISavingGoalsRepository
{
    Task<IEnumerable<dynamic>> GetAllAsync(int userId);
    Task<SavingGoal?> GetByIdAsync(int userId, int savingGoalId);
    Task<int> InsertAsync(SavingGoal goal);
    Task<bool> UpdateAsync(SavingGoal goal);
    Task<bool> DeactivateAsync(int userId, int savingGoalId);
}

public sealed class SavingGoalsRepository(ISqlConnectionFactory factory) : ISavingGoalsRepository
{
    public async Task<IEnumerable<dynamic>> GetAllAsync(int userId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT sg.SavingGoalId, sg.Name, sg.TargetAmount, sg.TargetDate, sg.PlannedMonthlyContribution, sg.IsActive,
                   ISNULL((SELECT SUM(sc.Amount) FROM SavingContributions sc WHERE sc.SavingGoalId = sg.SavingGoalId), 0) AS AccumulatedAmount
            FROM SavingGoals sg WHERE sg.UserId = @UserId AND sg.IsActive = 1 ORDER BY sg.SortOrder, sg.Name
            """, new { UserId = userId });
    }

    public async Task<SavingGoal?> GetByIdAsync(int userId, int savingGoalId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<SavingGoal>("SELECT SavingGoalId, UserId, CategoryId, Name, TargetAmount, TargetDate, PlannedMonthlyContribution, IsActive, SortOrder, CreatedAt FROM SavingGoals WHERE UserId = @UserId AND SavingGoalId = @SavingGoalId", new { UserId = userId, SavingGoalId = savingGoalId });
    }

    public async Task<int> InsertAsync(SavingGoal goal)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO SavingGoals (UserId, CategoryId, Name, TargetAmount, TargetDate, PlannedMonthlyContribution) OUTPUT INSERTED.SavingGoalId VALUES (@UserId, @CategoryId, @Name, @TargetAmount, @TargetDate, @PlannedMonthlyContribution)", goal);
    }

    public async Task<bool> UpdateAsync(SavingGoal goal)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE SavingGoals SET Name = @Name, TargetAmount = @TargetAmount, TargetDate = @TargetDate, PlannedMonthlyContribution = @PlannedMonthlyContribution WHERE UserId = @UserId AND SavingGoalId = @SavingGoalId", goal) > 0;
    }

    public async Task<bool> DeactivateAsync(int userId, int savingGoalId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE SavingGoals SET IsActive = 0 WHERE UserId = @UserId AND SavingGoalId = @SavingGoalId", new { UserId = userId, SavingGoalId = savingGoalId }) > 0;
    }
}

public interface ISavingContributionsRepository
{
    Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, int? savingGoalId);
    Task<SavingContribution?> GetByIdAsync(int userId, int savingContributionId);
    Task<int> InsertAsync(SavingContribution contribution);
    Task<bool> DeleteAsync(int userId, int savingContributionId);
}

public sealed class SavingContributionsRepository(ISqlConnectionFactory factory) : ISavingContributionsRepository
{
    public async Task<IEnumerable<dynamic>> GetByPeriodAsync(int userId, int monthlyPeriodId, int? savingGoalId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT sc.SavingContributionId, sc.MonthlyPeriodId, sc.SavingGoalId, sg.Name AS SavingGoalName, sc.Amount, sc.ContributionDate, sc.Note
            FROM SavingContributions sc INNER JOIN SavingGoals sg ON sg.SavingGoalId = sc.SavingGoalId
            WHERE sc.UserId = @UserId AND sc.MonthlyPeriodId = @MonthlyPeriodId AND (@SavingGoalId IS NULL OR sc.SavingGoalId = @SavingGoalId)
            ORDER BY sc.ContributionDate DESC
            """, new { UserId = userId, MonthlyPeriodId = monthlyPeriodId, SavingGoalId = savingGoalId });
    }

    public async Task<SavingContribution?> GetByIdAsync(int userId, int savingContributionId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<SavingContribution>("SELECT SavingContributionId, UserId, MonthlyPeriodId, SavingGoalId, Amount, ContributionDate, IsRecurring, RecurringTemplateId, Note, CreatedAt FROM SavingContributions WHERE UserId = @UserId AND SavingContributionId = @SavingContributionId", new { UserId = userId, SavingContributionId = savingContributionId });
    }

    public async Task<int> InsertAsync(SavingContribution contribution)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO SavingContributions (UserId, MonthlyPeriodId, SavingGoalId, Amount, ContributionDate, IsRecurring, RecurringTemplateId, Note) OUTPUT INSERTED.SavingContributionId VALUES (@UserId, @MonthlyPeriodId, @SavingGoalId, @Amount, @ContributionDate, @IsRecurring, @RecurringTemplateId, @Note)", contribution);
    }

    public async Task<bool> DeleteAsync(int userId, int savingContributionId)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("DELETE FROM SavingContributions WHERE UserId = @UserId AND SavingContributionId = @SavingContributionId", new { UserId = userId, SavingContributionId = savingContributionId }) > 0;
    }
}

public interface IRecurringTemplatesRepository
{
    Task<IEnumerable<dynamic>> GetAllAsync(int userId);
    Task<IEnumerable<RecurringTemplate>> GetActiveAsync(int userId);
    Task<RecurringTemplate?> GetByIdAsync(int userId, int recurringTemplateId);
    Task<int> InsertAsync(RecurringTemplate template);
    Task<bool> UpdateAsync(RecurringTemplate template);
    Task<bool> SetActiveAsync(int userId, int recurringTemplateId, bool isActive);
    Task<bool> DeleteAsync(int userId, int recurringTemplateId);
}

public sealed class RecurringTemplatesRepository(ISqlConnectionFactory factory) : IRecurringTemplatesRepository
{
    public async Task<IEnumerable<dynamic>> GetAllAsync(int userId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync("""
            SELECT rt.RecurringTemplateId, rt.TargetType, rt.CategoryId, c.Name AS CategoryName, rt.SavingGoalId, sg.Name AS SavingGoalName,
                   rt.Name, rt.Amount, rt.Classification, rt.DueDay, rt.IsActive
            FROM RecurringTemplates rt
            LEFT JOIN Categories c ON c.CategoryId = rt.CategoryId
            LEFT JOIN SavingGoals sg ON sg.SavingGoalId = rt.SavingGoalId
            WHERE rt.UserId = @UserId
            ORDER BY rt.TargetType, rt.SortOrder, rt.Name
            """, new { UserId = userId });
    }

    public async Task<IEnumerable<RecurringTemplate>> GetActiveAsync(int userId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync<RecurringTemplate>("SELECT RecurringTemplateId, UserId, TargetType, CategoryId, SavingGoalId, Name, Amount, Classification, DueDay, IsActive, SortOrder, CreatedAt, UpdatedAt FROM RecurringTemplates WHERE UserId = @UserId AND IsActive = 1", new { UserId = userId });
    }

    public async Task<RecurringTemplate?> GetByIdAsync(int userId, int recurringTemplateId)
    {
        using var c = factory.CreateConnection();
        return await c.QuerySingleOrDefaultAsync<RecurringTemplate>("SELECT RecurringTemplateId, UserId, TargetType, CategoryId, SavingGoalId, Name, Amount, Classification, DueDay, IsActive, SortOrder, CreatedAt, UpdatedAt FROM RecurringTemplates WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", new { UserId = userId, RecurringTemplateId = recurringTemplateId });
    }

    public async Task<int> InsertAsync(RecurringTemplate template)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteScalarAsync<int>("INSERT INTO RecurringTemplates (UserId, TargetType, CategoryId, SavingGoalId, Name, Amount, Classification, DueDay) OUTPUT INSERTED.RecurringTemplateId VALUES (@UserId, @TargetType, @CategoryId, @SavingGoalId, @Name, @Amount, @Classification, @DueDay)", template);
    }

    public async Task<bool> UpdateAsync(RecurringTemplate template)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE RecurringTemplates SET Name = @Name, Amount = @Amount, Classification = @Classification, DueDay = @DueDay, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", template) > 0;
    }

    public async Task<bool> SetActiveAsync(int userId, int recurringTemplateId, bool isActive)
    {
        using var c = factory.CreateConnection();
        return await c.ExecuteAsync("UPDATE RecurringTemplates SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME() WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", new { UserId = userId, RecurringTemplateId = recurringTemplateId, IsActive = isActive }) > 0;
    }

    public async Task<bool> DeleteAsync(int userId, int recurringTemplateId)
    {
        using var c = factory.CreateConnection();
        c.Open();
        using var tx = c.BeginTransaction();
        var p = new { UserId = userId, RecurringTemplateId = recurringTemplateId };
        await c.ExecuteAsync("UPDATE IncomeEntries SET RecurringTemplateId = NULL WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", p, tx);
        await c.ExecuteAsync("UPDATE ExpenseEntries SET RecurringTemplateId = NULL WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", p, tx);
        await c.ExecuteAsync("UPDATE BudgetAllocations SET RecurringTemplateId = NULL WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", p, tx);
        await c.ExecuteAsync("UPDATE SavingContributions SET RecurringTemplateId = NULL WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", p, tx);
        var deleted = await c.ExecuteAsync("DELETE FROM RecurringTemplates WHERE UserId = @UserId AND RecurringTemplateId = @RecurringTemplateId", p, tx);
        tx.Commit();
        return deleted > 0;
    }
}

public interface ISummaryRepository
{
    Task<MonthSummaryRow> GetSummaryAsync(int monthlyPeriodId, int year, int month);
    Task<IEnumerable<TrendPointRow>> GetTrendAsync(int userId, int months);
    Task<IEnumerable<CategoryBreakdownRow>> GetCategoryBreakdownAsync(int monthlyPeriodId);
}

public sealed class MonthSummaryRow
{
    public decimal TotalIncome { get; set; }
    public decimal SimpleExpenses { get; set; }
    public decimal TransactionExpenses { get; set; }
    public decimal FixedExpenses { get; set; }
    public decimal VariableExpenses { get; set; }
    public decimal TotalBudgetAllocation { get; set; }
    public decimal TotalBudgetActualUsed { get; set; }
    public decimal TotalSavings { get; set; }
    public int PaidExpenseCount { get; set; }
    public int UnpaidExpenseCount { get; set; }
}

public sealed class TrendPointRow
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalSavings { get; set; }
}

public sealed class CategoryBreakdownRow
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class SummaryRepository(ISqlConnectionFactory factory) : ISummaryRepository
{
    public async Task<MonthSummaryRow> GetSummaryAsync(int monthlyPeriodId, int year, int month)
    {
        using var c = factory.CreateConnection();
        const string sql = """
            SELECT
                (SELECT ISNULL(SUM(Amount), 0) FROM IncomeEntries WHERE MonthlyPeriodId = @MonthlyPeriodId AND IsActive = 1) AS TotalIncome,
                (SELECT ISNULL(SUM(Amount), 0) FROM ExpenseEntries WHERE MonthlyPeriodId = @MonthlyPeriodId) AS SimpleExpenses,
                (SELECT ISNULL(SUM(t.Amount), 0) FROM Transactions t INNER JOIN Categories cat ON cat.CategoryId = t.CategoryId WHERE t.MonthlyPeriodId = @MonthlyPeriodId AND cat.Type = 'Expense') AS TransactionExpenses,
                (SELECT ISNULL(SUM(Amount), 0) FROM (
                    SELECT ee.Amount FROM ExpenseEntries ee WHERE ee.MonthlyPeriodId = @MonthlyPeriodId AND ee.Classification = 'Fixed'
                    UNION ALL
                    SELECT t.Amount FROM Transactions t INNER JOIN Categories cat ON cat.CategoryId = t.CategoryId WHERE t.MonthlyPeriodId = @MonthlyPeriodId AND cat.Type = 'Expense' AND cat.Classification = 'Fixed'
                ) f) AS FixedExpenses,
                (SELECT ISNULL(SUM(Amount), 0) FROM (
                    SELECT ee.Amount FROM ExpenseEntries ee WHERE ee.MonthlyPeriodId = @MonthlyPeriodId AND ee.Classification = 'Variable'
                    UNION ALL
                    SELECT t.Amount FROM Transactions t INNER JOIN Categories cat ON cat.CategoryId = t.CategoryId WHERE t.MonthlyPeriodId = @MonthlyPeriodId AND cat.Type = 'Expense' AND ISNULL(cat.Classification, 'Variable') = 'Variable'
                ) v) AS VariableExpenses,
                (SELECT ISNULL(SUM(AllocatedAmount), 0) FROM BudgetAllocations WHERE MonthlyPeriodId = @MonthlyPeriodId) AS TotalBudgetAllocation,
                (SELECT ISNULL(SUM(t.Amount), 0) FROM Transactions t INNER JOIN BudgetAllocations ba ON ba.MonthlyPeriodId = t.MonthlyPeriodId AND ba.CategoryId = t.CategoryId WHERE t.MonthlyPeriodId = @MonthlyPeriodId) AS TotalBudgetActualUsed,
                (SELECT ISNULL(SUM(Amount), 0) FROM SavingContributions WHERE MonthlyPeriodId = @MonthlyPeriodId) AS TotalSavings,
                (SELECT COUNT(1) FROM ExpenseEntries WHERE MonthlyPeriodId = @MonthlyPeriodId AND IsPaid = 1) AS PaidExpenseCount,
                (SELECT COUNT(1) FROM ExpenseEntries WHERE MonthlyPeriodId = @MonthlyPeriodId AND IsPaid = 0) AS UnpaidExpenseCount
            """;
        var row = await c.QuerySingleAsync<MonthSummaryRow>(sql, new { MonthlyPeriodId = monthlyPeriodId, Year = year, Month = month });
        return row;
    }

    public async Task<IEnumerable<TrendPointRow>> GetTrendAsync(int userId, int months)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync<TrendPointRow>("""
            SELECT TOP (@Months)
                mp.Year, mp.Month,
                (SELECT ISNULL(SUM(ie.Amount), 0) FROM IncomeEntries ie WHERE ie.MonthlyPeriodId = mp.MonthlyPeriodId AND ie.IsActive = 1) AS TotalIncome,
                (SELECT ISNULL(SUM(ee.Amount), 0) FROM ExpenseEntries ee WHERE ee.MonthlyPeriodId = mp.MonthlyPeriodId)
                  + (SELECT ISNULL(SUM(t.Amount), 0) FROM Transactions t INNER JOIN Categories cat ON cat.CategoryId = t.CategoryId WHERE t.MonthlyPeriodId = mp.MonthlyPeriodId AND cat.Type = 'Expense') AS TotalExpenses,
                (SELECT ISNULL(SUM(sc.Amount), 0) FROM SavingContributions sc WHERE sc.MonthlyPeriodId = mp.MonthlyPeriodId) AS TotalSavings
            FROM MonthlyPeriods mp
            WHERE mp.UserId = @UserId
            ORDER BY mp.Year DESC, mp.Month DESC
            """, new { UserId = userId, Months = months });
    }

    public async Task<IEnumerable<CategoryBreakdownRow>> GetCategoryBreakdownAsync(int monthlyPeriodId)
    {
        using var c = factory.CreateConnection();
        return await c.QueryAsync<CategoryBreakdownRow>("""
            SELECT CategoryId, Name, Type, SUM(Amount) AS Amount FROM (
                SELECT c.CategoryId, c.Name, c.Type, ee.Amount
                FROM ExpenseEntries ee INNER JOIN Categories c ON c.CategoryId = ee.CategoryId
                WHERE ee.MonthlyPeriodId = @MonthlyPeriodId
                UNION ALL
                SELECT c.CategoryId, c.Name, c.Type, t.Amount
                FROM Transactions t INNER JOIN Categories c ON c.CategoryId = t.CategoryId
                WHERE t.MonthlyPeriodId = @MonthlyPeriodId AND c.Type = 'Expense'
            ) x
            GROUP BY CategoryId, Name, Type
            ORDER BY SUM(Amount) DESC
            """, new { MonthlyPeriodId = monthlyPeriodId });
    }
}
