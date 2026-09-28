using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyFlow.Api.Auth;
using MoneyFlow.Api.Common;
using MoneyFlow.Api.Dtos;
using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController(CategoriesService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> List([FromQuery] string? type) =>
        Ok(ApiResponse<IEnumerable<CategoryDto>>.Ok(await service.ListAsync(currentUser.UserId, type)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<CategoryDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPut("{categoryId:int}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int categoryId, UpdateCategoryRequest request) =>
        Ok(ApiResponse<CategoryDto>.Ok(await service.UpdateAsync(currentUser.UserId, categoryId, request)));

    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        await service.DeleteAsync(currentUser.UserId, categoryId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/incomes")]
public sealed class IncomesController(IncomesService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<IncomeEntryDto>>>> List([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<IEnumerable<IncomeEntryDto>>.Ok(await service.ListAsync(currentUser.UserId, monthlyPeriodId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<IncomeEntryDto>>> Create(CreateIncomeRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<IncomeEntryDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPut("{incomeEntryId:int}")]
    public async Task<ActionResult<ApiResponse<IncomeEntryDto>>> Update(int incomeEntryId, UpdateIncomeRequest request) =>
        Ok(ApiResponse<IncomeEntryDto>.Ok(await service.UpdateAsync(currentUser.UserId, incomeEntryId, request)));

    [HttpPatch("{incomeEntryId:int}/active")]
    public async Task<IActionResult> SetActive(int incomeEntryId, SetActiveRequest request)
    {
        await service.SetActiveAsync(currentUser.UserId, incomeEntryId, request.IsActive);
        return NoContent();
    }

    [HttpDelete("{incomeEntryId:int}")]
    public async Task<IActionResult> Delete(int incomeEntryId)
    {
        await service.DeleteAsync(currentUser.UserId, incomeEntryId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/expenses")]
public sealed class ExpensesController(ExpensesService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseEntryDto>>>> List([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<IEnumerable<ExpenseEntryDto>>.Ok(await service.ListAsync(currentUser.UserId, monthlyPeriodId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseEntryDto>>> Create(CreateExpenseRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<ExpenseEntryDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(ReorderExpensesRequest request)
    {
        await service.ReorderAsync(currentUser.UserId, request);
        return NoContent();
    }

    [HttpPut("{expenseEntryId:int}")]
    public async Task<ActionResult<ApiResponse<ExpenseEntryDto>>> Update(int expenseEntryId, UpdateExpenseRequest request) =>
        Ok(ApiResponse<ExpenseEntryDto>.Ok(await service.UpdateAsync(currentUser.UserId, expenseEntryId, request)));

    [HttpPatch("{expenseEntryId:int}/paid")]
    public async Task<IActionResult> SetPaid(int expenseEntryId, SetPaidRequest request)
    {
        await service.SetPaidAsync(currentUser.UserId, expenseEntryId, request);
        return NoContent();
    }

    [HttpDelete("{expenseEntryId:int}")]
    public async Task<IActionResult> Delete(int expenseEntryId)
    {
        await service.DeleteAsync(currentUser.UserId, expenseEntryId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/budgets")]
public sealed class BudgetsController(BudgetsService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BudgetAllocationDto>>>> List([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<IEnumerable<BudgetAllocationDto>>.Ok(await service.ListAsync(currentUser.UserId, monthlyPeriodId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetAllocationDto>>> Create(CreateBudgetRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<BudgetAllocationDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPut("{budgetAllocationId:int}")]
    public async Task<ActionResult<ApiResponse<BudgetAllocationDto>>> Update(int budgetAllocationId, UpdateBudgetRequest request) =>
        Ok(ApiResponse<BudgetAllocationDto>.Ok(await service.UpdateAsync(currentUser.UserId, budgetAllocationId, request)));

    [HttpDelete("{budgetAllocationId:int}")]
    public async Task<IActionResult> Delete(int budgetAllocationId)
    {
        await service.DeleteAsync(currentUser.UserId, budgetAllocationId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/transactions")]
public sealed class TransactionsController(TransactionsService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> List([FromQuery] int monthlyPeriodId, [FromQuery] int? categoryId) =>
        Ok(ApiResponse<IEnumerable<TransactionDto>>.Ok(await service.ListAsync(currentUser.UserId, monthlyPeriodId, categoryId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Create(CreateTransactionRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<TransactionDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPost("quick-add")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> QuickAdd(QuickAddTransactionRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<TransactionDto>.Ok(await service.QuickAddAsync(currentUser.UserId, request)));

    [HttpPut("{transactionId:int}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> Update(int transactionId, UpdateTransactionRequest request) =>
        Ok(ApiResponse<TransactionDto>.Ok(await service.UpdateAsync(currentUser.UserId, transactionId, request)));

    [HttpDelete("{transactionId:int}")]
    public async Task<IActionResult> Delete(int transactionId)
    {
        await service.DeleteAsync(currentUser.UserId, transactionId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/savings")]
public sealed class SavingsController(SavingsService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("goals")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SavingGoalDto>>>> Goals() =>
        Ok(ApiResponse<IEnumerable<SavingGoalDto>>.Ok(await service.ListGoalsAsync(currentUser.UserId)));

    [HttpPost("goals")]
    public async Task<ActionResult<ApiResponse<SavingGoalDto>>> CreateGoal(CreateSavingGoalRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<SavingGoalDto>.Ok(await service.CreateGoalAsync(currentUser.UserId, request)));

    [HttpPut("goals/{savingGoalId:int}")]
    public async Task<ActionResult<ApiResponse<SavingGoalDto>>> UpdateGoal(int savingGoalId, UpdateSavingGoalRequest request) =>
        Ok(ApiResponse<SavingGoalDto>.Ok(await service.UpdateGoalAsync(currentUser.UserId, savingGoalId, request)));

    [HttpDelete("goals/{savingGoalId:int}")]
    public async Task<IActionResult> DeleteGoal(int savingGoalId)
    {
        await service.DeleteGoalAsync(currentUser.UserId, savingGoalId);
        return NoContent();
    }

    [HttpGet("contributions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SavingContributionDto>>>> Contributions([FromQuery] int monthlyPeriodId, [FromQuery] int? savingGoalId) =>
        Ok(ApiResponse<IEnumerable<SavingContributionDto>>.Ok(await service.ListContributionsAsync(currentUser.UserId, monthlyPeriodId, savingGoalId)));

    [HttpPost("contributions")]
    public async Task<ActionResult<ApiResponse<SavingContributionDto>>> CreateContribution(CreateSavingContributionRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<SavingContributionDto>.Ok(await service.CreateContributionAsync(currentUser.UserId, request)));

    [HttpDelete("contributions/{savingContributionId:int}")]
    public async Task<IActionResult> DeleteContribution(int savingContributionId)
    {
        await service.DeleteContributionAsync(currentUser.UserId, savingContributionId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/recurring-items")]
public sealed class RecurringItemsController(RecurringService service, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RecurringTemplateDto>>>> List() =>
        Ok(ApiResponse<IEnumerable<RecurringTemplateDto>>.Ok(await service.ListAsync(currentUser.UserId)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RecurringTemplateDto>>> Create(CreateRecurringTemplateRequest request) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<RecurringTemplateDto>.Ok(await service.CreateAsync(currentUser.UserId, request)));

    [HttpPut("{recurringTemplateId:int}")]
    public async Task<ActionResult<ApiResponse<RecurringTemplateDto>>> Update(int recurringTemplateId, UpdateRecurringTemplateRequest request) =>
        Ok(ApiResponse<RecurringTemplateDto>.Ok(await service.UpdateAsync(currentUser.UserId, recurringTemplateId, request)));

    [HttpPatch("{recurringTemplateId:int}/active")]
    public async Task<IActionResult> SetActive(int recurringTemplateId, SetActiveRequest request)
    {
        await service.SetActiveAsync(currentUser.UserId, recurringTemplateId, request.IsActive);
        return NoContent();
    }

    [HttpDelete("{recurringTemplateId:int}")]
    public async Task<IActionResult> Delete(int recurringTemplateId)
    {
        await service.DeleteAsync(currentUser.UserId, recurringTemplateId);
        return NoContent();
    }
}

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(MonthsService months, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<MonthSummaryDto>>> Get([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<MonthSummaryDto>.Ok(await months.GetSummaryAsync(currentUser.UserId, monthlyPeriodId)));
}

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(ReportsService reports, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("trend")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TrendPointDto>>>> Trend([FromQuery] int months = 6) =>
        Ok(ApiResponse<IEnumerable<TrendPointDto>>.Ok(await reports.TrendAsync(currentUser.UserId, months)));

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryBreakdownDto>>>> Breakdown([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<IEnumerable<CategoryBreakdownDto>>.Ok(await reports.BreakdownAsync(currentUser.UserId, monthlyPeriodId)));

    [HttpGet("fixed-vs-variable")]
    public async Task<ActionResult<ApiResponse<FixedVsVariableDto>>> FixedVsVariable([FromQuery] int monthlyPeriodId) =>
        Ok(ApiResponse<FixedVsVariableDto>.Ok(await reports.FixedVsVariableAsync(currentUser.UserId, monthlyPeriodId)));
}
