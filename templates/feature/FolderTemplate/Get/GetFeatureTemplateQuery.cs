namespace FinanceTracker.Application.Features.DbSetPlaceholder.Get;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record GetFeatureTemplateQuery(Guid Id) : IQuery<Result<FeatureTemplateDetailResponse>>;

public sealed record FeatureTemplateDetailResponse(Guid Id);
