using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

namespace FinanceTracker.Application.Features.Categories.Delete;

public sealed class DeleteCategoryCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> HandleAsync(DeleteCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Categories.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("Category.NotFound", $"Category with ID '{command.Id}' was not found."));

        dbContext.Categories.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync($"categories:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
