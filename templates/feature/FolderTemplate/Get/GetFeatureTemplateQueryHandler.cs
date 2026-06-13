namespace FinanceTracker.Application.Features.DbSetPlaceholder.Get;

using Abstractions.Data;
using Abstractions.Identity;
using Abstractions.Messaging;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

public sealed class GetFeatureTemplateQueryHandler(
    IAppDbContext dbContext,
    ICurrentUser currentUser,
    HybridCache cache) : IQueryHandler<GetFeatureTemplateQuery, Result<FeatureTemplateDetailResponse>>
{
    public async Task<Result<FeatureTemplateDetailResponse>> HandleAsync(GetFeatureTemplateQuery query, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;
        var response = await cache.GetOrCreateAsync<FeatureTemplateDetailResponse?>(
            $"featuretemplate:{userId}:{query.Id}",
            async ct => await dbContext.DbSetPlaceholder
                .Where(e => e.Id == query.Id && e.UserId == userId)
                .Select(e => new FeatureTemplateDetailResponse(e.Id))
                .FirstOrDefaultAsync(ct),
            tags: [$"featuretemplates:{userId}"],
            cancellationToken: cancellationToken);

        return response is null
            ? Result.Failure<FeatureTemplateDetailResponse>(Error.NotFound("FeatureTemplate.NotFound", $"FeatureTemplate with ID '{query.Id}' was not found."))
            : Result.Success(response);
    }
}
