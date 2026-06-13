namespace FinanceTracker.Application.Features.Transactions.Delete;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class DeleteTransactionCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<DeleteTransactionCommand>
{
    public async Task<Result> HandleAsync(DeleteTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Transactions.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("Transaction.NotFound", $"Transaction with ID '{command.Id}' was not found."));

        dbContext.Transactions.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"transactions:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
