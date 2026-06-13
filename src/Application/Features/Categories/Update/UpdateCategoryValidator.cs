using FluentValidation;

namespace FinanceTracker.Application.Features.Categories.Update;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.Type).IsInEnum();
    }
}
