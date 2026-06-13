using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Categories.Get;

public sealed record GetCategoryQuery(Guid Id) : IQuery<Result<CategoryDetailResponse>>;

public sealed record CategoryDetailResponse(Guid Id, string Name, CategoryType Type);
