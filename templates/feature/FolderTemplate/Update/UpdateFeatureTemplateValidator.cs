namespace FinanceTracker.Application.Features.DbSetPlaceholder.Update;

using FluentValidation;

public sealed class UpdateFeatureTemplateValidator : AbstractValidator<UpdateFeatureTemplateCommand>
{
    public UpdateFeatureTemplateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
