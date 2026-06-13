namespace FinanceTracker.Application.Features.Budgets.GetAll;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Application.Features.Budgets.Get;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetAllBudgetQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetAllBudgetQuery, Result<PagedResult<BudgetDetailResponse>>>
{
    public async Task<Result<PagedResult<BudgetDetailResponse>>> HandleAsync(GetAllBudgetQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var result = await cache.GetOrCreateAsync(
            $"budgets:all:{userId}:{query.Page}:{query.PageSize}",
            async ct =>
            {
                var baseQuery = dbContext.Budgets.Where(e => e.UserId == userId);

                var totalCount = await baseQuery.CountAsync(ct);

                var items = await baseQuery
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(e => new BudgetDetailResponse(e.Id, e.Limit, e.Month, e.Year, e.CategoryId))
                    .ToListAsync(ct);

                return new PagedResult<BudgetDetailResponse>(items, totalCount, query.Page, query.PageSize);
            },
            tags: [$"budgets:{userId}"],
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }
}
