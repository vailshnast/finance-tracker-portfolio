namespace FinanceTracker.Application.Features.Transactions.Get;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetTransactionQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetTransactionQuery, Result<TransactionDetailResponse>>
{
    public async Task<Result<TransactionDetailResponse>> HandleAsync(GetTransactionQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var response = await cache.GetOrCreateAsync<TransactionDetailResponse?>(
            $"transaction:{userId}:{query.Id}",
            async ct => await dbContext.Transactions
                .Where(e => e.Id == query.Id && e.UserId == userId)
                .Select(e => new TransactionDetailResponse(e.Id, e.Amount, e.Description, e.CategoryId, e.CreatedAt))
                .FirstOrDefaultAsync(ct),
            tags: [$"transactions:{userId}"],
            cancellationToken: cancellationToken);

        return response is null
            ? Result.Failure<TransactionDetailResponse>(Error.NotFound("Transaction.NotFound", $"Transaction with ID '{query.Id}' was not found."))
            : Result.Success(response);
    }
}
