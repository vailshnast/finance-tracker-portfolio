namespace FinanceTracker.Application.Features.Transactions.Update;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class UpdateTransactionCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<UpdateTransactionCommand>
{
    public async Task<Result> HandleAsync(UpdateTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Transactions.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("Transaction.NotFound", $"Transaction with ID '{command.Id}' was not found."));

        entity.Date = command.Date;
        entity.Amount = command.Amount;
        entity.Description = command.Description;
        entity.CategoryId = command.CategoryId;

        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"transactions:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
