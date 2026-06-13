namespace FinanceTracker.Application.Features.Budgets.Create;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class CreateBudgetCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<CreateBudgetCommand, Result<CreateBudgetResponse>>
{
    public async Task<Result<CreateBudgetResponse>> HandleAsync(CreateBudgetCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await dbContext.Budgets.AnyAsync(
            b => b.UserId == currentUser.UserId
                 && b.CategoryId == command.CategoryId
                 && b.Month == command.Month
                 && b.Year == command.Year,
            cancellationToken);

        if (exists)
            return Result.Failure<CreateBudgetResponse>(Error.Conflict(
                "Budget.Conflict",
                $"A budget for this category in {command.Month}/{command.Year} already exists."));

        var entity = new Budget
        {
            Limit = command.Limit,
            Month = command.Month,
            Year = command.Year,
            CategoryId = command.CategoryId,
            UserId = currentUser.UserId!
        };

        dbContext.Budgets.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"budgets:{currentUser.UserId}", cancellationToken);

        return Result.Success(new CreateBudgetResponse(entity.Id));
    }
}
