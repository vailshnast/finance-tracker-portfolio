namespace FinanceTracker.Application.Features.DbSetPlaceholder.Create;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record CreateFeatureTemplateCommand() : ICommand<Result<CreateFeatureTemplateResponse>>;

public sealed record CreateFeatureTemplateResponse(Guid Id);
