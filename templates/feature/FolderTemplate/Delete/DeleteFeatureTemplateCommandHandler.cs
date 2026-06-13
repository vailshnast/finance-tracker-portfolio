namespace FinanceTracker.Application.Features.DbSetPlaceholder.Delete;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class DeleteFeatureTemplateCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<DeleteFeatureTemplateCommand>
{
    public async Task<Result> HandleAsync(DeleteFeatureTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.DbSetPlaceholder.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("FeatureTemplate.NotFound", $"FeatureTemplate with ID '{command.Id}' was not found."));

        dbContext.DbSetPlaceholder.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"featuretemplates:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
