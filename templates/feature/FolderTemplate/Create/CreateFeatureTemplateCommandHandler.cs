namespace FinanceTracker.Application.Features.DbSetPlaceholder.Create;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Domain.Entities;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class CreateFeatureTemplateCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<CreateFeatureTemplateCommand, Result<CreateFeatureTemplateResponse>>
{
    public async Task<Result<CreateFeatureTemplateResponse>> HandleAsync(CreateFeatureTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var entity = new EntityPlaceholder
        {
            UserId = currentUser.UserId!
        };

        dbContext.DbSetPlaceholder.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"featuretemplates:{currentUser.UserId}", cancellationToken);

        return Result.Success(new CreateFeatureTemplateResponse(entity.Id));
    }
}
