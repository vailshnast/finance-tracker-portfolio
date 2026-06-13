namespace FinanceTracker.Application.Features.Budgets.Get;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetBudgetQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetBudgetQuery, Result<BudgetDetailResponse>>
{
    public async Task<Result<BudgetDetailResponse>> HandleAsync(GetBudgetQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var response = await cache.GetOrCreateAsync<BudgetDetailResponse?>(
            $"budget:{userId}:{query.Id}",
            async ct => await dbContext.Budgets
                .Where(e => e.Id == query.Id && e.UserId == userId)
                .Select(e => new BudgetDetailResponse(e.Id, e.Limit, e.Month, e.Year, e.CategoryId))
                .FirstOrDefaultAsync(ct),
            tags: [$"budgets:{userId}"],
            cancellationToken: cancellationToken);

        return response is null
            ? Result.Failure<BudgetDetailResponse>(Error.NotFound("Budget.NotFound", $"Budget with ID '{query.Id}' was not found."))
            : Result.Success(response);
    }
}
