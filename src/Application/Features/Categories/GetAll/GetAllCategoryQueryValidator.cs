using FluentValidation;

namespace FinanceTracker.Application.Features.Categories.GetAll;

public sealed class GetAllCategoryQueryValidator : AbstractValidator<GetAllCategoryQuery>
{
    public GetAllCategoryQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
    }
}
