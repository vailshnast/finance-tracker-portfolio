namespace FinanceTracker.Application.Features.Budgets.Create;

using FluentValidation;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.Limit).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}
