namespace FinanceTracker.Application.Features.Budgets.Delete;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class DeleteBudgetCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<DeleteBudgetCommand>
{
    public async Task<Result> HandleAsync(DeleteBudgetCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Budgets.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("Budget.NotFound", $"Budget with ID '{command.Id}' was not found."));

        dbContext.Budgets.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"budgets:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
