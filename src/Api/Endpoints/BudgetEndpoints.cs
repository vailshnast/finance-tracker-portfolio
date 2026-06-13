using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Budgets.Create;
using FinanceTracker.Application.Features.Budgets.Delete;
using FinanceTracker.Application.Features.Budgets.Get;
using FinanceTracker.Application.Features.Budgets.GetAll;
using FinanceTracker.Application.Features.Budgets.Update;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.Endpoints;

public static class BudgetEndpoints
{
    public static void MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/budgets")
            .WithTags("Budget")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .AddEndpointFilter<ValidationFilter<GetAllBudgetQuery>>()
            .WithName("GetAllBudget")
            .WithSummary("Get all Budgets with pagination")
            .WithDescription("Returns a paginated list of Budgets for the authenticated user. " +
                             "Use `page` and `pageSize` query parameters to control pagination. " +
                             "Defaults to page 1 with 10 items per page.")
            .Produces<PagedResult<BudgetDetailResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetBudgetById")
            .WithSummary("Get a Budget by ID")
            .WithDescription("Returns a single Budget by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces<BudgetDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateBudgetCommand>>()
            .WithName("CreateBudget")
            .WithSummary("Create a new Budget")
            .WithDescription("Creates a new Budget for the authenticated user. " +
                             "Returns the created resource with its assigned ID and a `Location` header pointing to the new resource.")
            .Produces<CreateBudgetResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateBudgetCommand>>()
            .WithName("UpdateBudget")
            .WithSummary("Update an existing Budget")
            .WithDescription("Updates an existing Budget by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteBudget")
            .WithSummary("Delete a Budget")
            .WithDescription("Permanently deletes a Budget by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetAllBudgetQuery query,
        IQueryHandler<GetAllBudgetQuery, Result<PagedResult<BudgetDetailResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetBudgetQuery, Result<BudgetDetailResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetBudgetQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateBudgetCommand command,
        ICommandHandler<CreateBudgetCommand, Result<CreateBudgetResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetBudgetById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateBudgetCommand request,
        ICommandHandler<UpdateBudgetCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteBudgetCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteBudgetCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
