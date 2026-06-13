namespace FinanceTracker.Application.Features.DbSetPlaceholder.GetAll;

using Application.Abstractions.Messaging;
using Application.Features.DbSetPlaceholder.Get;
using Domain.Common;

public sealed record GetAllFeatureTemplateQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<FeatureTemplateDetailResponse>>>;
