namespace FinanceTracker.Application.Features.Budgets.Update;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class UpdateBudgetCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<UpdateBudgetCommand>
{
    public async Task<Result> HandleAsync(UpdateBudgetCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Budgets.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("Budget.NotFound", $"Budget with ID '{command.Id}' was not found."));

        var conflict = await dbContext.Budgets.AnyAsync(
            b => b.Id != command.Id
                 && b.UserId == currentUser.UserId
                 && b.CategoryId == command.CategoryId
                 && b.Month == command.Month
                 && b.Year == command.Year,
            cancellationToken);

        if (conflict)
            return Result.Failure(Error.Conflict(
                "Budget.Conflict",
                $"A budget for this category in {command.Month}/{command.Year} already exists."));

        entity.Limit = command.Limit;
        entity.Month = command.Month;
        entity.Year = command.Year;
        entity.CategoryId = command.CategoryId;

        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"budgets:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
