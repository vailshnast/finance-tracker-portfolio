using FluentValidation;

namespace FinanceTracker.Application.Features.Categories.Create;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Type).IsInEnum();
    }
}
