using FluentValidation;

namespace FinanceTracker.Application.Features.Budgets.GetAll;

public sealed class GetAllBudgetQueryValidator : AbstractValidator<GetAllBudgetQuery>
{
    public GetAllBudgetQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
