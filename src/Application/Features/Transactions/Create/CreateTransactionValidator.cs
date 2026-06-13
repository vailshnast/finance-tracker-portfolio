namespace FinanceTracker.Application.Features.Transactions.Create;

using FluentValidation;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
    }
}
