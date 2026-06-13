using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using Microsoft.Extensions.Caching.Hybrid;

namespace FinanceTracker.Application.Features.Categories.Create;

public sealed class CreateCategoryCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<CreateCategoryCommand, Result<CreateCategoryResponse>>
{
    public async Task<Result<CreateCategoryResponse>> HandleAsync(CreateCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new Category
        {
            Name = command.Name,
            Type = command.Type,
            UserId = currentUser.UserId!
        };

        dbContext.Categories.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await cache.RemoveByTagAsync($"categories:{currentUser.UserId}", cancellationToken);

        return Result.Success(new CreateCategoryResponse(entity.Id, entity.Name, entity.Type));
    }
}
