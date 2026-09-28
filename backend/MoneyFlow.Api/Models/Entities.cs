namespace MoneyFlow.Api.Models;

public sealed class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int PeriodStartDay { get; set; } = 26;
    public bool SkipWeekendPayday { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class Category
{
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Classification { get; set; }
    public string TrackingMode { get; set; } = "Simple";
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class MonthlyPeriod
{
    public int MonthlyPeriodId { get; set; }
    public int UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RecurringTemplate
{
    public int RecurringTemplateId { get; set; }
    public int UserId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int? SavingGoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Classification { get; set; }
    public int? DueDay { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class IncomeEntry
{
    public int IncomeEntryId { get; set; }
    public int UserId { get; set; }
    public int MonthlyPeriodId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime IncomeDate { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringTemplateId { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class ExpenseEntry
{
    public int ExpenseEntryId { get; set; }
    public int UserId { get; set; }
    public int MonthlyPeriodId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Classification { get; set; } = "Variable";
    public DateTime? DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringTemplateId { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class BudgetAllocation
{
    public int BudgetAllocationId { get; set; }
    public int UserId { get; set; }
    public int MonthlyPeriodId { get; set; }
    public int CategoryId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringTemplateId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class TransactionEntry
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public int MonthlyPeriodId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class SavingGoal
{
    public int SavingGoalId { get; set; }
    public int UserId { get; set; }
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal? PlannedMonthlyContribution { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SavingContribution
{
    public int SavingContributionId { get; set; }
    public int UserId { get; set; }
    public int MonthlyPeriodId { get; set; }
    public int SavingGoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringTemplateId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
