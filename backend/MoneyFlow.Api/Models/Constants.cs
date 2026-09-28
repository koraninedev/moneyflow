namespace MoneyFlow.Api.Models;

public static class CategoryType
{
    public const string Income = "Income";
    public const string Expense = "Expense";
    public const string Saving = "Saving";
}

public static class Classification
{
    public const string Fixed = "Fixed";
    public const string Variable = "Variable";
}

public static class TrackingMode
{
    public const string Simple = "Simple";
    public const string BudgetVsActual = "BudgetVsActual";
}

public static class RecurringTargetType
{
    public const string Income = "Income";
    public const string ExpenseEntry = "ExpenseEntry";
    public const string BudgetAllocation = "BudgetAllocation";
    public const string SavingContribution = "SavingContribution";
}
