using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Categories.Create;
using FinanceTracker.Application.Features.Categories.Delete;
using FinanceTracker.Application.Features.Categories.Get;
using FinanceTracker.Application.Features.Categories.GetAll;
using FinanceTracker.Application.Features.Categories.Update;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Category")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .AddEndpointFilter<ValidationFilter<GetAllCategoryQuery>>()
            .WithName("GetAllCategory")
            .WithSummary("Get all Categorys with pagination")
            .WithDescription("Returns a paginated list of Categorys for the authenticated user. " +
                             "Use `page` and `pageSize` query parameters to control pagination. " +
                             "Defaults to page 1 with 10 items per page.")
            .Produces<PagedResult<CategoryDetailResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetCategoryById")
            .WithSummary("Get a Category by ID")
            .WithDescription("Returns a single Category by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces<CategoryDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateCategoryCommand>>()
            .WithName("CreateCategory")
            .WithSummary("Create a new Category")
            .WithDescription("Creates a new Category for the authenticated user. " +
                             "Returns the created resource with its assigned ID and a `Location` header pointing to the new resource.")
            .Produces<CreateCategoryResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateCategoryCommand>>()
            .WithName("UpdateCategory")
            .WithSummary("Update an existing Category")
            .WithDescription("Updates an existing Category by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteCategory")
            .WithSummary("Delete a Category")
            .WithDescription("Permanently deletes a Category by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetAllCategoryQuery query,
        IQueryHandler<GetAllCategoryQuery, Result<PagedResult<CategoryDetailResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetCategoryQuery, Result<CategoryDetailResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCategoryQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateCategoryCommand command,
        ICommandHandler<CreateCategoryCommand, Result<CreateCategoryResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetCategoryById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateCategoryCommand request,
        ICommandHandler<UpdateCategoryCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteCategoryCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteCategoryCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
