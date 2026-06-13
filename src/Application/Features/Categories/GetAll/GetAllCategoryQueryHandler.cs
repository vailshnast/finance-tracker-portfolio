using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Categories.Get;
using FinanceTracker.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Features.Categories.GetAll;

public sealed class GetAllCategoryQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetAllCategoryQuery, Result<PagedResult<CategoryDetailResponse>>>
{
    public async Task<Result<PagedResult<CategoryDetailResponse>>> HandleAsync(GetAllCategoryQuery query, CancellationToken cancellationToken = default)
    {
         var userId = currentUser.UserId;
         var result = await cache.GetOrCreateAsync(
             $"categories:all:{userId}:{query.Page}:{query.PageSize}",
             async ct =>
             {
                 var baseQuery = dbContext.Categories.Where(c => c.UserId == userId);
                 var totalCount = await baseQuery.CountAsync(ct);

                 var items = await baseQuery
                     .OrderByDescending(c => c.CreatedAt)
                     .Skip((query.Page - 1) * query.PageSize)
                     .Take(query.PageSize)
                     .Select(c => new CategoryDetailResponse(c.Id, c.Name, c.Type))
                     .ToListAsync(ct);

                 return new PagedResult<CategoryDetailResponse>(items, totalCount, query.Page, query.PageSize);
             },
             tags: [$"categories:{userId}"],
             cancellationToken: cancellationToken);

         return Result.Success(result);
    }
}
