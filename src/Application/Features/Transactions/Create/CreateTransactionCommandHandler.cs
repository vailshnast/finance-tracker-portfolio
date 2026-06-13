namespace FinanceTracker.Application.Features.Transactions.Create;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class CreateTransactionCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<CreateTransactionCommand, Result<CreateTransactionResponse>>
{
    public async Task<Result<CreateTransactionResponse>> HandleAsync(CreateTransactionCommand command, CancellationToken cancellationToken = default)
    {
        var category = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == command.CategoryId && c.UserId == currentUser.UserId, cancellationToken);

        if (category is null)
            return Result.Failure<CreateTransactionResponse>(
                Error.NotFound("Category.NotFound", "Category not found."));

        var entity = new Transaction
        {
            Date = command.Date,
            Amount = command.Amount,
            Description = command.Description,
            CategoryId = command.CategoryId,
            UserId = currentUser.UserId!
        };

        dbContext.Transactions.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"transactions:{currentUser.UserId}", cancellationToken);

        return Result.Success(new CreateTransactionResponse(entity.Id));
    }
}
