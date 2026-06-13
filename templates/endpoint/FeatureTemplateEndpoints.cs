using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.DbSetPlaceholder.Create;
using FinanceTracker.Application.Features.DbSetPlaceholder.Delete;
using FinanceTracker.Application.Features.DbSetPlaceholder.Get;
using FinanceTracker.Application.Features.DbSetPlaceholder.GetAll;
using FinanceTracker.Application.Features.DbSetPlaceholder.Update;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.Endpoints;

public static class FeatureTemplateEndpoints
{
    public static void MapFeatureTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/featuretemplates")
            .WithTags("FeatureTemplate")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .AddEndpointFilter<ValidationFilter<GetAllFeatureTemplateQuery>>()
            .WithName("GetAllFeatureTemplate")
            .WithSummary("Get all FeatureTemplates with pagination")
            .WithDescription("Returns a paginated list of FeatureTemplates for the authenticated user. " +
                             "Use `page` and `pageSize` query parameters to control pagination. " +
                             "Defaults to page 1 with 10 items per page.")
            .Produces<PagedResult<FeatureTemplateDetailResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetFeatureTemplateById")
            .WithSummary("Get a FeatureTemplate by ID")
            .WithDescription("Returns a single FeatureTemplate by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces<FeatureTemplateDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateFeatureTemplateCommand>>()
            .WithName("CreateFeatureTemplate")
            .WithSummary("Create a new FeatureTemplate")
            .WithDescription("Creates a new FeatureTemplate for the authenticated user. " +
                             "Returns the created resource with its assigned ID and a `Location` header pointing to the new resource.")
            .Produces<CreateFeatureTemplateResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateFeatureTemplateCommand>>()
            .WithName("UpdateFeatureTemplate")
            .WithSummary("Update an existing FeatureTemplate")
            .WithDescription("Updates an existing FeatureTemplate by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteFeatureTemplate")
            .WithSummary("Delete a FeatureTemplate")
            .WithDescription("Permanently deletes a FeatureTemplate by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetAllFeatureTemplateQuery query,
        IQueryHandler<GetAllFeatureTemplateQuery, Result<PagedResult<FeatureTemplateDetailResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetFeatureTemplateQuery, Result<FeatureTemplateDetailResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetFeatureTemplateQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateFeatureTemplateCommand command,
        ICommandHandler<CreateFeatureTemplateCommand, Result<CreateFeatureTemplateResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetFeatureTemplateById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateFeatureTemplateCommand request,
        ICommandHandler<UpdateFeatureTemplateCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteFeatureTemplateCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteFeatureTemplateCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
