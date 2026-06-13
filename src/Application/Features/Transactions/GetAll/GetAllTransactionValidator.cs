using FluentValidation;

namespace FinanceTracker.Application.Features.Transactions.GetAll;

public sealed class GetAllTransactionValidator : AbstractValidator<GetAllTransactionQuery>
{
    public GetAllTransactionValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(q => q.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}
