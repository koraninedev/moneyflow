namespace MoneyFlow.Api.Dtos;

public sealed record RegisterRequest(string Email, string Password, string DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record UserDto(int UserId, string Email, string DisplayName, int PeriodStartDay, bool SkipWeekendPayday);
public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);
public sealed record UpdateSettingsRequest(int PeriodStartDay, bool SkipWeekendPayday);

public sealed record CategoryDto(int CategoryId, string Name, string Type, string? Classification, string TrackingMode, string? Icon, string? Color, int SortOrder, bool IsActive);
public sealed record CreateCategoryRequest(string Name, string Type, string? Classification, string? TrackingMode, string? Icon, string? Color);
public sealed record UpdateCategoryRequest(string Name, string? Classification, string TrackingMode, string? Icon, string? Color, int SortOrder);

public sealed record MonthListItemDto(int MonthlyPeriodId, int Year, int Month, decimal TotalIncome, decimal TotalExpenses, decimal TotalSavings, decimal Remaining);
public sealed record CreateMonthRequest(int Year, int Month, string Mode);
public sealed record MonthProgressDto(int DayOfMonth, int DaysInMonth, int RemainingDays, double PercentElapsed, DateTime PeriodStart, DateTime PeriodEnd);
public sealed record CategoryBudgetDto(int CategoryId, string Name, decimal Allocated, decimal Used, decimal Remaining, decimal TodayUsed);
public sealed record MonthSummaryDto(int MonthlyPeriodId, int Year, int Month, decimal TotalIncome, decimal TotalExpenses, decimal FixedExpenses, decimal VariableExpenses, decimal TotalBudgetAllocation, decimal TotalBudgetActualUsed, decimal TotalSavings, decimal Remaining, int PaidExpenseCount, int UnpaidExpenseCount, MonthProgressDto MonthProgress, List<CategoryBudgetDto> CategoryBudgets);

public sealed record IncomeEntryDto(int IncomeEntryId, int MonthlyPeriodId, int CategoryId, string CategoryName, string Name, decimal Amount, DateTime IncomeDate, bool IsRecurring, string? Note, bool IsActive);
public sealed record CreateIncomeRequest(int MonthlyPeriodId, int CategoryId, string Name, decimal Amount, DateTime IncomeDate, bool IsRecurring, string? Note);
public sealed record UpdateIncomeRequest(int CategoryId, string Name, decimal Amount, DateTime IncomeDate, bool IsRecurring, string? Note);
public sealed record SetActiveRequest(bool IsActive);

public sealed record ExpenseEntryDto(int ExpenseEntryId, int MonthlyPeriodId, int CategoryId, string CategoryName, string Name, decimal Amount, string Classification, DateTime? DueDate, bool IsPaid, DateTime? PaidDate, bool IsRecurring, string? Note, int SortOrder);
public sealed record CreateExpenseRequest(int MonthlyPeriodId, int CategoryId, string Name, decimal Amount, string Classification, DateTime? DueDate, string? Note);
public sealed record UpdateExpenseRequest(int CategoryId, string Name, decimal Amount, string Classification, DateTime? DueDate, string? Note, int SortOrder);
public sealed record SetPaidRequest(bool IsPaid, DateTime? PaidDate);
public sealed record ReorderExpensesRequest(int MonthlyPeriodId, List<int> OrderedIds);

public sealed record BudgetAllocationDto(int BudgetAllocationId, int MonthlyPeriodId, int CategoryId, string CategoryName, decimal AllocatedAmount, decimal Used, decimal Remaining, decimal TodayUsed, string? Note);
public sealed record CreateBudgetRequest(int MonthlyPeriodId, int CategoryId, decimal AllocatedAmount, string? Note);
public sealed record UpdateBudgetRequest(decimal AllocatedAmount, string? Note);

public sealed record TransactionDto(int TransactionId, int MonthlyPeriodId, int CategoryId, string CategoryName, decimal Amount, DateTime TransactionDate, string Description, string? Note);
public sealed record CreateTransactionRequest(int MonthlyPeriodId, int CategoryId, decimal Amount, DateTime TransactionDate, string Description, string? Note);
public sealed record QuickAddTransactionRequest(int CategoryId, decimal Amount, string Description);
public sealed record UpdateTransactionRequest(int CategoryId, decimal Amount, DateTime TransactionDate, string Description, string? Note);

public sealed record SavingGoalDto(int SavingGoalId, string Name, decimal? TargetAmount, DateTime? TargetDate, decimal? PlannedMonthlyContribution, decimal AccumulatedAmount, bool IsActive);
public sealed record CreateSavingGoalRequest(string Name, decimal? TargetAmount, DateTime? TargetDate, decimal? PlannedMonthlyContribution);
public sealed record UpdateSavingGoalRequest(string Name, decimal? TargetAmount, DateTime? TargetDate, decimal? PlannedMonthlyContribution);
public sealed record SavingContributionDto(int SavingContributionId, int MonthlyPeriodId, int SavingGoalId, string SavingGoalName, decimal Amount, DateTime ContributionDate, string? Note);
public sealed record CreateSavingContributionRequest(int MonthlyPeriodId, int SavingGoalId, decimal Amount, DateTime ContributionDate, string? Note);

public sealed record RecurringTemplateDto(int RecurringTemplateId, string TargetType, int? CategoryId, string? CategoryName, int? SavingGoalId, string? SavingGoalName, string Name, decimal Amount, string? Classification, int? DueDay, bool IsActive);
public sealed record CreateRecurringTemplateRequest(string TargetType, int? CategoryId, int? SavingGoalId, string Name, decimal Amount, string? Classification, int? DueDay);
public sealed record UpdateRecurringTemplateRequest(string Name, decimal Amount, string? Classification, int? DueDay);

public sealed record TrendPointDto(int Year, int Month, decimal TotalIncome, decimal TotalExpenses, decimal TotalSavings, decimal Remaining);
public sealed record CategoryBreakdownDto(int CategoryId, string Name, string Type, decimal Amount);
public sealed record FixedVsVariableDto(decimal Fixed, decimal Variable);
