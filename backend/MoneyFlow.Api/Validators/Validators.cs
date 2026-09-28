using FluentValidation;
using MoneyFlow.Api.Dtos;

namespace MoneyFlow.Api.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.DisplayName).NotEmpty();
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class UpdateSettingsRequestValidator : AbstractValidator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.PeriodStartDay).InclusiveBetween(1, 28);
    }
}

public sealed class CreateIncomeRequestValidator : AbstractValidator<CreateIncomeRequest>
{
    public CreateIncomeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.MonthlyPeriodId).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

public sealed class CreateExpenseRequestValidator : AbstractValidator<CreateExpenseRequest>
{
    public CreateExpenseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Classification).NotEmpty();
    }
}

public sealed class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator() => RuleFor(x => x.AllocatedAmount).GreaterThanOrEqualTo(0);
}

public sealed class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
    }
}

public sealed class QuickAddTransactionRequestValidator : AbstractValidator<QuickAddTransactionRequest>
{
    public QuickAddTransactionRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
    }
}

public sealed class CreateSavingContributionRequestValidator : AbstractValidator<CreateSavingContributionRequest>
{
    public CreateSavingContributionRequestValidator() => RuleFor(x => x.Amount).GreaterThan(0);
}

public sealed class CreateMonthRequestValidator : AbstractValidator<CreateMonthRequest>
{
    public CreateMonthRequestValidator()
    {
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Mode).Must(m => m is "Empty" or "CopyRecurring");
    }
}
