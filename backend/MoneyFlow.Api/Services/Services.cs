using MoneyFlow.Api.Auth;
using MoneyFlow.Api.Common;
using MoneyFlow.Api.Dtos;
using MoneyFlow.Api.Models;
using MoneyFlow.Api.Repositories;

namespace MoneyFlow.Api.Services;

public sealed class AuthService(IUsersRepository users, IPasswordHasher hasher, IJwtTokenService jwt, ICategoriesRepository categories)
{
    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Invalid registration data.", new Dictionary<string, string> { ["password"] = "Must be at least 8 characters." });
        if (await users.GetByEmailAsync(request.Email) is not null)
            throw new ConflictException("Email already registered.");
        var userId = await users.InsertAsync(new User { Email = request.Email.Trim(), PasswordHash = hasher.Hash(request.Password), DisplayName = request.DisplayName.Trim(), PeriodStartDay = PayCycle.DefaultPaydayDay, SkipWeekendPayday = true });
        await categories.SeedDefaultsForUserAsync(userId);
        return ToUserDto(userId, request.Email.Trim(), request.DisplayName.Trim(), PayCycle.DefaultPaydayDay, true);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await users.GetByEmailAsync(request.Email);
        if (user is null || !user.IsActive || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");
        var (token, expiresAt) = jwt.IssueToken(user.UserId, user.Email);
        return new LoginResponse(token, expiresAt, ToDto(user));
    }

    public async Task<UserDto> MeAsync(int userId)
    {
        var user = await users.GetByIdAsync(userId) ?? throw new NotFoundException();
        return ToDto(user);
    }

    public async Task<UserDto> UpdateSettingsAsync(int userId, UpdateSettingsRequest request)
    {
        if (request.PeriodStartDay is < 1 or > 28) throw new ValidationException("Period start day must be 1-28.");
        if (!await users.UpdateSettingsAsync(userId, request.PeriodStartDay, request.SkipWeekendPayday)) throw new NotFoundException();
        return await MeAsync(userId);
    }

    static UserDto ToDto(User user) => ToUserDto(user.UserId, user.Email, user.DisplayName, user.PeriodStartDay, user.SkipWeekendPayday);
    static UserDto ToUserDto(int userId, string email, string displayName, int periodStartDay, bool skipWeekendPayday) =>
        new(userId, email, displayName, periodStartDay, skipWeekendPayday);
}

public sealed class CategoriesService(ICategoriesRepository repo)
{
    public async Task<IEnumerable<CategoryDto>> ListAsync(int userId, string? type)
    {
        var rows = await repo.GetAllAsync(userId, type);
        return rows.Select(c => new CategoryDto(c.CategoryId, c.Name, c.Type, c.Classification, c.TrackingMode, c.Icon, c.Color, c.SortOrder, c.IsActive));
    }

    public async Task<CategoryDto> CreateAsync(int userId, CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ValidationException("Name is required.");
        var id = await repo.InsertAsync(new Category { UserId = userId, Name = request.Name.Trim(), Type = request.Type, Classification = request.Classification, TrackingMode = string.IsNullOrWhiteSpace(request.TrackingMode) ? TrackingMode.Simple : request.TrackingMode, Icon = request.Icon, Color = request.Color });
        var created = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        return new CategoryDto(created.CategoryId, created.Name, created.Type, created.Classification, created.TrackingMode, created.Icon, created.Color, created.SortOrder, created.IsActive);
    }

    public async Task<CategoryDto> UpdateAsync(int userId, int categoryId, UpdateCategoryRequest request)
    {
        var existing = await repo.GetByIdAsync(userId, categoryId) ?? throw new NotFoundException();
        existing.Name = request.Name.Trim();
        existing.Classification = request.Classification;
        existing.TrackingMode = request.TrackingMode;
        existing.Icon = request.Icon;
        existing.Color = request.Color;
        existing.SortOrder = request.SortOrder;
        await repo.UpdateAsync(existing);
        return new CategoryDto(existing.CategoryId, existing.Name, existing.Type, existing.Classification, existing.TrackingMode, existing.Icon, existing.Color, existing.SortOrder, existing.IsActive);
    }

    public async Task DeleteAsync(int userId, int categoryId)
    {
        if (!await repo.DeactivateAsync(userId, categoryId)) throw new NotFoundException();
    }
}

public sealed class MonthsService(IMonthlyPeriodsRepository periods, IRecurringTemplatesRepository templates, IIncomeEntriesRepository incomes, IExpenseEntriesRepository expenses, IBudgetAllocationsRepository budgets, ISavingContributionsRepository contributions, ISummaryRepository summary, IUsersRepository users)
{
    public async Task<IEnumerable<MonthListItemDto>> ListAsync(int userId)
    {
        var list = new List<MonthListItemDto>();
        foreach (var p in await periods.GetAllAsync(userId))
        {
            var s = await BuildSummaryAsync(p);
            list.Add(new MonthListItemDto(p.MonthlyPeriodId, p.Year, p.Month, s.TotalIncome, s.TotalExpenses, s.TotalSavings, s.Remaining));
        }
        return list;
    }

    public async Task<MonthSummaryDto> GetByYearMonthAsync(int userId, int year, int month)
    {
        var p = await periods.GetByYearMonthAsync(userId, year, month) ?? throw new NotFoundException();
        return await BuildSummaryAsync(p);
    }

    public async Task<MonthSummaryDto> GetCurrentAsync(int userId)
    {
        var user = await users.GetByIdAsync(userId) ?? throw new NotFoundException();
        var today = PayCycle.TodayInBangkok();
        var (year, month) = PayCycle.PeriodContaining(today, user.PeriodStartDay, user.SkipWeekendPayday);
        var existing = await periods.GetByYearMonthAsync(userId, year, month);
        if (existing is null)
        {
            var id = await periods.InsertAsync(userId, year, month);
            existing = await periods.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        }
        return await BuildSummaryAsync(existing);
    }

    public async Task<MonthSummaryDto> GetSummaryAsync(int userId, int monthlyPeriodId)
    {
        var p = await periods.GetByIdAsync(userId, monthlyPeriodId) ?? throw new NotFoundException();
        return await BuildSummaryAsync(p);
    }

    public async Task<MonthSummaryDto> CreateAsync(int userId, CreateMonthRequest request)
    {
        if (request.Month is < 1 or > 12) throw new ValidationException("Month must be 1-12.");
        if (await periods.ExistsAsync(userId, request.Year, request.Month)) throw new ConflictException("Month already exists.");
        var id = await periods.InsertAsync(userId, request.Year, request.Month);
        if (string.Equals(request.Mode, "CopyRecurring", StringComparison.OrdinalIgnoreCase))
            await CopyRecurringAsync(userId, id, request.Year, request.Month);
        var p = await periods.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        return await BuildSummaryAsync(p);
    }

    public async Task CopyRecurringAsync(int userId, int monthlyPeriodId, int year, int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        foreach (var t in await templates.GetActiveAsync(userId))
        {
            var due = t.DueDay.HasValue ? Math.Min(t.DueDay.Value, daysInMonth) : Math.Min(1, daysInMonth);
            var date = new DateTime(year, month, due);
            switch (t.TargetType)
            {
                case RecurringTargetType.Income:
                    await incomes.InsertAsync(new IncomeEntry { UserId = userId, MonthlyPeriodId = monthlyPeriodId, CategoryId = t.CategoryId!.Value, Name = t.Name, Amount = t.Amount, IncomeDate = date, IsRecurring = true, RecurringTemplateId = t.RecurringTemplateId });
                    break;
                case RecurringTargetType.ExpenseEntry:
                    await expenses.InsertAsync(new ExpenseEntry { UserId = userId, MonthlyPeriodId = monthlyPeriodId, CategoryId = t.CategoryId!.Value, Name = t.Name, Amount = t.Amount, Classification = t.Classification ?? Classification.Fixed, DueDate = date, IsRecurring = true, RecurringTemplateId = t.RecurringTemplateId });
                    break;
                case RecurringTargetType.BudgetAllocation:
                    if (await budgets.GetByPeriodAndCategoryAsync(userId, monthlyPeriodId, t.CategoryId!.Value) is null)
                        await budgets.InsertAsync(new BudgetAllocation { UserId = userId, MonthlyPeriodId = monthlyPeriodId, CategoryId = t.CategoryId!.Value, AllocatedAmount = t.Amount, IsRecurring = true, RecurringTemplateId = t.RecurringTemplateId });
                    break;
                case RecurringTargetType.SavingContribution:
                    await contributions.InsertAsync(new SavingContribution { UserId = userId, MonthlyPeriodId = monthlyPeriodId, SavingGoalId = t.SavingGoalId!.Value, Amount = t.Amount, ContributionDate = date, IsRecurring = true, RecurringTemplateId = t.RecurringTemplateId });
                    break;
            }
        }
    }

    public async Task<MonthSummaryDto> BuildSummaryAsync(MonthlyPeriod p)
    {
        var row = await summary.GetSummaryAsync(p.MonthlyPeriodId, p.Year, p.Month);
        var totalExpenses = Accounting.TotalExpenses(row.SimpleExpenses, row.TransactionExpenses);
        var remaining = Accounting.Remaining(row.TotalIncome, totalExpenses, row.TotalSavings);
        var user = await users.GetByIdAsync(p.UserId) ?? throw new NotFoundException();
        var today = PayCycle.TodayInBangkok();
        var progress = PayCycle.Progress(today, p.Year, p.Month, user.PeriodStartDay, user.SkipWeekendPayday);
        var budgetRows = await budgets.GetByPeriodAsync(p.UserId, p.MonthlyPeriodId, today);
        var categoryBudgets = budgetRows.Select(r =>
        {
            decimal allocated = r.AllocatedAmount;
            decimal used = r.Used;
            decimal todayUsed = r.TodayUsed;
            return new CategoryBudgetDto((int)r.CategoryId, (string)r.CategoryName, allocated, used, Accounting.CategoryRemaining(allocated, used), todayUsed);
        }).ToList();
        return new MonthSummaryDto(p.MonthlyPeriodId, p.Year, p.Month, row.TotalIncome, totalExpenses, row.FixedExpenses, row.VariableExpenses, row.TotalBudgetAllocation, row.TotalBudgetActualUsed, row.TotalSavings, remaining, row.PaidExpenseCount, row.UnpaidExpenseCount, new MonthProgressDto(progress.DayOfPeriod, progress.DaysInPeriod, progress.RemainingDays, progress.PercentElapsed, progress.Start, progress.End), categoryBudgets);
    }
}

public sealed class IncomesService(IIncomeEntriesRepository repo, IMonthlyPeriodsRepository periods, ICategoriesRepository categories)
{
    public async Task<IEnumerable<IncomeEntryDto>> ListAsync(int userId, int monthlyPeriodId)
    {
        await EnsurePeriod(userId, monthlyPeriodId);
        var rows = await repo.GetByPeriodAsync(userId, monthlyPeriodId);
        return rows.Select(r => new IncomeEntryDto((int)r.IncomeEntryId, (int)r.MonthlyPeriodId, (int)r.CategoryId, (string)r.CategoryName, (string)r.Name, (decimal)r.Amount, (DateTime)r.IncomeDate, (bool)r.IsRecurring, (string?)r.Note, (bool)r.IsActive));
    }

    public async Task<IncomeEntryDto> CreateAsync(int userId, CreateIncomeRequest request)
    {
        if (request.Amount <= 0) throw new ValidationException("Amount must be greater than 0.", new Dictionary<string, string> { ["amount"] = "Must be greater than 0" });
        await EnsurePeriod(userId, request.MonthlyPeriodId);
        var cat = await categories.GetByIdAsync(userId, request.CategoryId) ?? throw new ValidationException("Invalid category.");
        if (cat.Type != CategoryType.Income) throw new ValidationException("Category must be Income.");
        var id = await repo.InsertAsync(new IncomeEntry { UserId = userId, MonthlyPeriodId = request.MonthlyPeriodId, CategoryId = request.CategoryId, Name = request.Name, Amount = request.Amount, IncomeDate = request.IncomeDate.Date, IsRecurring = request.IsRecurring, Note = request.Note });
        var created = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        return new IncomeEntryDto(created.IncomeEntryId, created.MonthlyPeriodId, created.CategoryId, cat.Name, created.Name, created.Amount, created.IncomeDate, created.IsRecurring, created.Note, created.IsActive);
    }

    public async Task<IncomeEntryDto> UpdateAsync(int userId, int id, UpdateIncomeRequest request)
    {
        var existing = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        var cat = await categories.GetByIdAsync(userId, request.CategoryId) ?? throw new ValidationException("Invalid category.");
        existing.CategoryId = request.CategoryId; existing.Name = request.Name; existing.Amount = request.Amount; existing.IncomeDate = request.IncomeDate.Date; existing.IsRecurring = request.IsRecurring; existing.Note = request.Note;
        await repo.UpdateAsync(existing);
        return new IncomeEntryDto(existing.IncomeEntryId, existing.MonthlyPeriodId, existing.CategoryId, cat.Name, existing.Name, existing.Amount, existing.IncomeDate, existing.IsRecurring, existing.Note, existing.IsActive);
    }

    public async Task SetActiveAsync(int userId, int id, bool isActive)
    {
        if (!await repo.SetActiveAsync(userId, id, isActive)) throw new NotFoundException();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        if (!await repo.DeleteAsync(userId, id)) throw new NotFoundException();
    }

    private async Task EnsurePeriod(int userId, int monthlyPeriodId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
    }
}

public sealed class ExpensesService(IExpenseEntriesRepository repo, IMonthlyPeriodsRepository periods, ICategoriesRepository categories)
{
    public async Task<IEnumerable<ExpenseEntryDto>> ListAsync(int userId, int monthlyPeriodId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
        var rows = await repo.GetByPeriodAsync(userId, monthlyPeriodId);
        return rows.Select(r => new ExpenseEntryDto((int)r.ExpenseEntryId, (int)r.MonthlyPeriodId, (int)r.CategoryId, (string)r.CategoryName, (string)r.Name, (decimal)r.Amount, (string)r.Classification, (DateTime?)r.DueDate, (bool)r.IsPaid, (DateTime?)r.PaidDate, (bool)r.IsRecurring, (string?)r.Note, (int)r.SortOrder));
    }

    public async Task<ExpenseEntryDto> CreateAsync(int userId, CreateExpenseRequest request)
    {
        if (request.Amount <= 0) throw new ValidationException("Amount must be greater than 0.", new Dictionary<string, string> { ["amount"] = "Must be greater than 0" });
        if (await periods.GetByIdAsync(userId, request.MonthlyPeriodId) is null) throw new NotFoundException();
        var cat = await categories.GetByIdAsync(userId, request.CategoryId) ?? throw new ValidationException("Invalid category.");
        if (cat.Type != CategoryType.Expense || cat.TrackingMode != TrackingMode.Simple)
            throw new ValidationException("Category must be Expense with TrackingMode Simple.");
        var id = await repo.InsertAsync(new ExpenseEntry { UserId = userId, MonthlyPeriodId = request.MonthlyPeriodId, CategoryId = request.CategoryId, Name = request.Name, Amount = request.Amount, Classification = request.Classification, DueDate = request.DueDate?.Date, Note = request.Note });
        var created = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        return new ExpenseEntryDto(created.ExpenseEntryId, created.MonthlyPeriodId, created.CategoryId, cat.Name, created.Name, created.Amount, created.Classification, created.DueDate, created.IsPaid, created.PaidDate, created.IsRecurring, created.Note, created.SortOrder);
    }

    public async Task<ExpenseEntryDto> UpdateAsync(int userId, int id, UpdateExpenseRequest request)
    {
        var existing = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        var cat = await categories.GetByIdAsync(userId, request.CategoryId) ?? throw new ValidationException("Invalid category.");
        if (cat.TrackingMode != TrackingMode.Simple) throw new ValidationException("Category must be Simple.");
        existing.CategoryId = request.CategoryId; existing.Name = request.Name; existing.Amount = request.Amount; existing.Classification = request.Classification; existing.DueDate = request.DueDate?.Date; existing.Note = request.Note; existing.SortOrder = request.SortOrder;
        await repo.UpdateAsync(existing);
        return new ExpenseEntryDto(existing.ExpenseEntryId, existing.MonthlyPeriodId, existing.CategoryId, cat.Name, existing.Name, existing.Amount, existing.Classification, existing.DueDate, existing.IsPaid, existing.PaidDate, existing.IsRecurring, existing.Note, existing.SortOrder);
    }

    public async Task SetPaidAsync(int userId, int id, SetPaidRequest request)
    {
        if (!await repo.SetPaidAsync(userId, id, request.IsPaid, request.IsPaid ? (request.PaidDate ?? DateTime.UtcNow.Date) : null)) throw new NotFoundException();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        if (!await repo.DeleteAsync(userId, id)) throw new NotFoundException();
    }

    public Task ReorderAsync(int userId, ReorderExpensesRequest request) => repo.ReorderAsync(userId, request.MonthlyPeriodId, request.OrderedIds);
}

public sealed class BudgetsService(IBudgetAllocationsRepository repo, IMonthlyPeriodsRepository periods, ICategoriesRepository categories)
{
    public async Task<IEnumerable<BudgetAllocationDto>> ListAsync(int userId, int monthlyPeriodId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
        var rows = await repo.GetByPeriodAsync(userId, monthlyPeriodId, PayCycle.TodayInBangkok());
        return rows.Select(r =>
        {
            decimal allocated = r.AllocatedAmount; decimal used = r.Used; decimal todayUsed = r.TodayUsed;
            return new BudgetAllocationDto((int)r.BudgetAllocationId, (int)r.MonthlyPeriodId, (int)r.CategoryId, (string)r.CategoryName, allocated, used, Accounting.CategoryRemaining(allocated, used), todayUsed, (string?)r.Note);
        });
    }

    public async Task<BudgetAllocationDto> CreateAsync(int userId, CreateBudgetRequest request)
    {
        if (request.AllocatedAmount < 0) throw new ValidationException("Allocated amount cannot be negative.");
        if (await periods.GetByIdAsync(userId, request.MonthlyPeriodId) is null) throw new NotFoundException();
        var cat = await categories.GetByIdAsync(userId, request.CategoryId) ?? throw new ValidationException("Invalid category.");
        if (cat.Type != CategoryType.Expense || cat.TrackingMode != TrackingMode.BudgetVsActual)
            throw new ValidationException("Category must be Expense with TrackingMode BudgetVsActual.");
        if (await repo.GetByPeriodAndCategoryAsync(userId, request.MonthlyPeriodId, request.CategoryId) is not null)
            throw new ConflictException("Allocation already exists for this category and month.");
        var id = await repo.InsertAsync(new BudgetAllocation { UserId = userId, MonthlyPeriodId = request.MonthlyPeriodId, CategoryId = request.CategoryId, AllocatedAmount = request.AllocatedAmount, Note = request.Note });
        return new BudgetAllocationDto(id, request.MonthlyPeriodId, request.CategoryId, cat.Name, request.AllocatedAmount, 0, request.AllocatedAmount, 0, request.Note);
    }

    public async Task<BudgetAllocationDto> UpdateAsync(int userId, int id, UpdateBudgetRequest request)
    {
        var existing = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        existing.AllocatedAmount = request.AllocatedAmount; existing.Note = request.Note;
        await repo.UpdateAsync(existing);
        var cat = await categories.GetByIdAsync(userId, existing.CategoryId);
        return new BudgetAllocationDto(existing.BudgetAllocationId, existing.MonthlyPeriodId, existing.CategoryId, cat?.Name ?? "", existing.AllocatedAmount, 0, existing.AllocatedAmount, 0, existing.Note);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        if (!await repo.DeleteAsync(userId, id)) throw new NotFoundException();
    }
}

public sealed class TransactionsService(ITransactionsRepository repo, IMonthlyPeriodsRepository periods, ICategoriesRepository categories)
{
    public async Task<IEnumerable<TransactionDto>> ListAsync(int userId, int monthlyPeriodId, int? categoryId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
        var rows = await repo.GetByPeriodAsync(userId, monthlyPeriodId, categoryId);
        return rows.Select(r => new TransactionDto((int)r.TransactionId, (int)r.MonthlyPeriodId, (int)r.CategoryId, (string)r.CategoryName, (decimal)r.Amount, (DateTime)r.TransactionDate, (string)r.Description, (string?)r.Note));
    }

    public async Task<TransactionDto> CreateAsync(int userId, CreateTransactionRequest request)
    {
        if (request.Amount <= 0) throw new ValidationException("Amount must be greater than 0.", new Dictionary<string, string> { ["amount"] = "Must be greater than 0" });
        EnsureNotFuture(request.TransactionDate);
        if (await periods.GetByIdAsync(userId, request.MonthlyPeriodId) is null) throw new NotFoundException();
        var cat = await EnsureBudgetCategory(userId, request.CategoryId);
        var id = await repo.InsertAsync(new TransactionEntry { UserId = userId, MonthlyPeriodId = request.MonthlyPeriodId, CategoryId = request.CategoryId, Amount = request.Amount, TransactionDate = request.TransactionDate.Date, Description = request.Description, Note = request.Note });
        return new TransactionDto(id, request.MonthlyPeriodId, request.CategoryId, cat.Name, request.Amount, request.TransactionDate.Date, request.Description, request.Note);
    }

    public async Task<TransactionDto> QuickAddAsync(int userId, QuickAddTransactionRequest request)
    {
        var now = DateTime.UtcNow.Date;
        var period = await periods.GetByYearMonthAsync(userId, now.Year, now.Month);
        if (period is null)
        {
            var id = await periods.InsertAsync(userId, now.Year, now.Month);
            period = await periods.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        }
        return await CreateAsync(userId, new CreateTransactionRequest(period.MonthlyPeriodId, request.CategoryId, request.Amount, now, request.Description, null));
    }

    public async Task<TransactionDto> UpdateAsync(int userId, int id, UpdateTransactionRequest request)
    {
        if (request.Amount <= 0) throw new ValidationException("Amount must be greater than 0.", new Dictionary<string, string> { ["amount"] = "Must be greater than 0" });
        EnsureNotFuture(request.TransactionDate);
        var existing = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        var cat = await EnsureBudgetCategory(userId, request.CategoryId);
        existing.CategoryId = request.CategoryId; existing.Amount = request.Amount; existing.TransactionDate = request.TransactionDate.Date; existing.Description = request.Description; existing.Note = request.Note;
        await repo.UpdateAsync(existing);
        return new TransactionDto(existing.TransactionId, existing.MonthlyPeriodId, existing.CategoryId, cat.Name, existing.Amount, existing.TransactionDate, existing.Description, existing.Note);
    }

    public async Task DeleteAsync(int userId, int id)
    {
        if (!await repo.DeleteAsync(userId, id)) throw new NotFoundException();
    }

    private async Task<Category> EnsureBudgetCategory(int userId, int categoryId)
    {
        var cat = await categories.GetByIdAsync(userId, categoryId) ?? throw new ValidationException("Invalid category.");
        if (cat.TrackingMode != TrackingMode.BudgetVsActual) throw new ValidationException("Category must have TrackingMode BudgetVsActual.");
        return cat;
    }

    private static void EnsureNotFuture(DateTime date)
    {
        if (date.Date > PayCycle.TodayInBangkok())
            throw new ValidationException("Cannot record a future date.", new Dictionary<string, string> { ["transactionDate"] = "Cannot be in the future" });
    }
}

public sealed class SavingsService(ISavingGoalsRepository goals, ISavingContributionsRepository contributions, IMonthlyPeriodsRepository periods)
{
    public async Task<IEnumerable<SavingGoalDto>> ListGoalsAsync(int userId)
    {
        var rows = await goals.GetAllAsync(userId);
        return rows.Select(r => new SavingGoalDto((int)r.SavingGoalId, (string)r.Name, (decimal?)r.TargetAmount, (DateTime?)r.TargetDate, (decimal?)r.PlannedMonthlyContribution, (decimal)r.AccumulatedAmount, (bool)r.IsActive));
    }

    public async Task<SavingGoalDto> CreateGoalAsync(int userId, CreateSavingGoalRequest request)
    {
        var id = await goals.InsertAsync(new SavingGoal { UserId = userId, Name = request.Name, TargetAmount = request.TargetAmount, TargetDate = request.TargetDate, PlannedMonthlyContribution = request.PlannedMonthlyContribution });
        return new SavingGoalDto(id, request.Name, request.TargetAmount, request.TargetDate, request.PlannedMonthlyContribution, 0, true);
    }

    public async Task<SavingGoalDto> UpdateGoalAsync(int userId, int id, UpdateSavingGoalRequest request)
    {
        var existing = await goals.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        existing.Name = request.Name; existing.TargetAmount = request.TargetAmount; existing.TargetDate = request.TargetDate; existing.PlannedMonthlyContribution = request.PlannedMonthlyContribution;
        await goals.UpdateAsync(existing);
        return new SavingGoalDto(existing.SavingGoalId, existing.Name, existing.TargetAmount, existing.TargetDate, existing.PlannedMonthlyContribution, 0, existing.IsActive);
    }

    public async Task DeleteGoalAsync(int userId, int id)
    {
        if (!await goals.DeactivateAsync(userId, id)) throw new NotFoundException();
    }

    public async Task<IEnumerable<SavingContributionDto>> ListContributionsAsync(int userId, int monthlyPeriodId, int? savingGoalId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
        var rows = await contributions.GetByPeriodAsync(userId, monthlyPeriodId, savingGoalId);
        return rows.Select(r => new SavingContributionDto((int)r.SavingContributionId, (int)r.MonthlyPeriodId, (int)r.SavingGoalId, (string)r.SavingGoalName, (decimal)r.Amount, (DateTime)r.ContributionDate, (string?)r.Note));
    }

    public async Task<SavingContributionDto> CreateContributionAsync(int userId, CreateSavingContributionRequest request)
    {
        if (request.Amount <= 0) throw new ValidationException("Amount must be greater than 0.");
        if (await periods.GetByIdAsync(userId, request.MonthlyPeriodId) is null) throw new NotFoundException();
        var goal = await goals.GetByIdAsync(userId, request.SavingGoalId) ?? throw new ValidationException("Invalid saving goal.");
        var id = await contributions.InsertAsync(new SavingContribution { UserId = userId, MonthlyPeriodId = request.MonthlyPeriodId, SavingGoalId = request.SavingGoalId, Amount = request.Amount, ContributionDate = request.ContributionDate.Date, Note = request.Note });
        return new SavingContributionDto(id, request.MonthlyPeriodId, request.SavingGoalId, goal.Name, request.Amount, request.ContributionDate.Date, request.Note);
    }

    public async Task DeleteContributionAsync(int userId, int id)
    {
        if (!await contributions.DeleteAsync(userId, id)) throw new NotFoundException();
    }
}

public sealed class RecurringService(IRecurringTemplatesRepository repo)
{
    public async Task<IEnumerable<RecurringTemplateDto>> ListAsync(int userId)
    {
        var rows = await repo.GetAllAsync(userId);
        return rows.Select(r => new RecurringTemplateDto((int)r.RecurringTemplateId, (string)r.TargetType, (int?)r.CategoryId, (string?)r.CategoryName, (int?)r.SavingGoalId, (string?)r.SavingGoalName, (string)r.Name, (decimal)r.Amount, (string?)r.Classification, (int?)r.DueDay, (bool)r.IsActive));
    }

    public async Task<RecurringTemplateDto> CreateAsync(int userId, CreateRecurringTemplateRequest request)
    {
        var id = await repo.InsertAsync(new RecurringTemplate { UserId = userId, TargetType = request.TargetType, CategoryId = request.CategoryId, SavingGoalId = request.SavingGoalId, Name = request.Name, Amount = request.Amount, Classification = request.Classification, DueDay = request.DueDay });
        return new RecurringTemplateDto(id, request.TargetType, request.CategoryId, null, request.SavingGoalId, null, request.Name, request.Amount, request.Classification, request.DueDay, true);
    }

    public async Task<RecurringTemplateDto> UpdateAsync(int userId, int id, UpdateRecurringTemplateRequest request)
    {
        var existing = await repo.GetByIdAsync(userId, id) ?? throw new NotFoundException();
        existing.Name = request.Name; existing.Amount = request.Amount; existing.Classification = request.Classification; existing.DueDay = request.DueDay;
        await repo.UpdateAsync(existing);
        return new RecurringTemplateDto(existing.RecurringTemplateId, existing.TargetType, existing.CategoryId, null, existing.SavingGoalId, null, existing.Name, existing.Amount, existing.Classification, existing.DueDay, existing.IsActive);
    }

    public async Task SetActiveAsync(int userId, int id, bool isActive)
    {
        if (!await repo.SetActiveAsync(userId, id, isActive)) throw new NotFoundException();
    }

    public async Task DeleteAsync(int userId, int id)
    {
        if (!await repo.DeleteAsync(userId, id)) throw new NotFoundException();
    }
}

public sealed class ReportsService(ISummaryRepository summary, IMonthlyPeriodsRepository periods)
{
    public async Task<IEnumerable<TrendPointDto>> TrendAsync(int userId, int months)
    {
        var rows = await summary.GetTrendAsync(userId, months);
        return rows.Reverse().Select(r => new TrendPointDto(r.Year, r.Month, r.TotalIncome, r.TotalExpenses, r.TotalSavings, Accounting.Remaining(r.TotalIncome, r.TotalExpenses, r.TotalSavings)));
    }

    public async Task<IEnumerable<CategoryBreakdownDto>> BreakdownAsync(int userId, int monthlyPeriodId)
    {
        if (await periods.GetByIdAsync(userId, monthlyPeriodId) is null) throw new NotFoundException();
        var rows = await summary.GetCategoryBreakdownAsync(monthlyPeriodId);
        return rows.Select(r => new CategoryBreakdownDto(r.CategoryId, r.Name, r.Type, r.Amount));
    }

    public async Task<FixedVsVariableDto> FixedVsVariableAsync(int userId, int monthlyPeriodId)
    {
        var p = await periods.GetByIdAsync(userId, monthlyPeriodId) ?? throw new NotFoundException();
        var row = await summary.GetSummaryAsync(p.MonthlyPeriodId, p.Year, p.Month);
        return new FixedVsVariableDto(row.FixedExpenses, row.VariableExpenses);
    }
}
