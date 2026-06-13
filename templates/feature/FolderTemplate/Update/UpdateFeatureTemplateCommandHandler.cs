namespace FinanceTracker.Application.Features.DbSetPlaceholder.Update;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class UpdateFeatureTemplateCommandHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : ICommandHandler<UpdateFeatureTemplateCommand>
{
    public async Task<Result> HandleAsync(UpdateFeatureTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.DbSetPlaceholder.FindAsync([command.Id], cancellationToken);

        if (entity is null || entity.UserId != currentUser.UserId)
            return Result.Failure(Error.NotFound("FeatureTemplate.NotFound", $"FeatureTemplate with ID '{command.Id}' was not found."));

        await dbContext.SaveChangesAsync(cancellationToken);

        await cache.RemoveByTagAsync($"featuretemplates:{currentUser.UserId}", cancellationToken);

        return Result.Success();
    }
}
