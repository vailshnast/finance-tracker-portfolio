using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Categories.Get;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Application.Features.Categories.GetAll;

public sealed record GetAllCategoryQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<CategoryDetailResponse>>>;
