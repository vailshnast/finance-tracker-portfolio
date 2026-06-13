using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace FinanceTracker.Application.Features.Categories.Get;

public sealed class GetCategoryQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetCategoryQuery, Result<CategoryDetailResponse>>
{
    public async Task<Result<CategoryDetailResponse>> HandleAsync(GetCategoryQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var cacheKey = $"category:{userId}:{query.Id}";

        var response = await cache.GetOrCreateAsync<CategoryDetailResponse?>(
            cacheKey,
            async ct => await dbContext.Categories
                .Where(c => c.Id == query.Id && c.UserId == userId)
                .Select(c => new CategoryDetailResponse(c.Id, c.Name, c.Type))
                .FirstOrDefaultAsync(ct),
            tags: [$"categories:{userId}"],
            cancellationToken: cancellationToken);

        if (response is null)
            return Result.Failure<CategoryDetailResponse>(Error.NotFound("Category.NotFound", $"Category with ID '{query.Id}' was not found."));

        return Result.Success(response);
    }
}
