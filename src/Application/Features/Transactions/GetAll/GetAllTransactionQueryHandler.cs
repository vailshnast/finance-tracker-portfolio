namespace FinanceTracker.Application.Features.Transactions.GetAll;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Application.Features.Transactions.Get;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetAllTransactionQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetAllTransactionQuery, Result<PagedResult<TransactionDetailResponse>>>
{
    public async Task<Result<PagedResult<TransactionDetailResponse>>> HandleAsync(GetAllTransactionQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        // Cache key only encodes pagination — adding filter params (date range, category) would require extending the key.
        var result = await cache.GetOrCreateAsync(
            $"transactions:all:{userId}:{query.Page}:{query.PageSize}",
            async ct =>
            {
                var baseQuery = dbContext.Transactions.Where(e => e.UserId == userId);

                var totalCount = await baseQuery.CountAsync(ct);

                var items = await baseQuery
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(e => new TransactionDetailResponse(e.Id, e.Amount, e.Description, e.CategoryId, e.Date))
                    .ToListAsync(ct);

                return new PagedResult<TransactionDetailResponse>(items, totalCount, query.Page, query.PageSize);
            },
            tags: [$"transactions:{userId}"],
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }
}
