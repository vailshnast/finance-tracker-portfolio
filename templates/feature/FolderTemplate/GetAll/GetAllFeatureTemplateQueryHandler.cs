namespace FinanceTracker.Application.Features.DbSetPlaceholder.GetAll;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Application.Features.DbSetPlaceholder.Get;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetAllFeatureTemplateQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetAllFeatureTemplateQuery, Result<PagedResult<FeatureTemplateDetailResponse>>>
{
    public async Task<Result<PagedResult<FeatureTemplateDetailResponse>>> HandleAsync(GetAllFeatureTemplateQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var result = await cache.GetOrCreateAsync(
            $"featuretemplates:all:{userId}:{query.Page}:{query.PageSize}",
            async ct =>
            {
                var baseQuery = dbContext.DbSetPlaceholder.Where(e => e.UserId == userId);

                var totalCount = await baseQuery.CountAsync(ct);

                var items = await baseQuery
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(e => new FeatureTemplateDetailResponse(e.Id))
                    .ToListAsync(ct);

                return new PagedResult<FeatureTemplateDetailResponse>(items, totalCount, query.Page, query.PageSize);
            },
            tags: [$"featuretemplates:{userId}"],
            cancellationToken: cancellationToken);

        return Result.Success(result);
    }
}
