using FinanceTracker.Api.Extensions;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Application.Features.Transactions.Create;
using FinanceTracker.Application.Features.Transactions.Delete;
using FinanceTracker.Application.Features.Transactions.Get;
using FinanceTracker.Application.Features.Transactions.GetAll;
using FinanceTracker.Application.Features.Transactions.Update;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transaction")
            .WithTags("Transaction")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .AddEndpointFilter<ValidationFilter<GetAllTransactionQuery>>()
            .WithName("GetAllTransaction")
            .WithSummary("Get all Transactions with pagination")
            .WithDescription("Returns a paginated list of Transactions for the authenticated user. " +
                             "Use `page` and `pageSize` query parameters to control pagination. " +
                             "Defaults to page 1 with 10 items per page.")
            .Produces<PagedResult<TransactionDetailResponse>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetTransactionById")
            .WithSummary("Get a Transaction by ID")
            .WithDescription("Returns a single Transaction by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces<TransactionDetailResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/", Create)
            .AddEndpointFilter<ValidationFilter<CreateTransactionCommand>>()
            .WithName("CreateTransaction")
            .WithSummary("Create a new Transaction")
            .WithDescription("Creates a new Transaction for the authenticated user. " +
                             "Returns the created resource with its assigned ID and a `Location` header pointing to the new resource.")
            .Produces<CreateTransactionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id:guid}", Update)
            .AddEndpointFilter<ValidationFilter<UpdateTransactionCommand>>()
            .WithName("UpdateTransaction")
            .WithSummary("Update an existing Transaction")
            .WithDescription("Updates an existing Transaction by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteTransaction")
            .WithSummary("Delete a Transaction")
            .WithDescription("Permanently deletes a Transaction by its unique identifier. " +
                             "Returns 404 if the resource does not exist or does not belong to the authenticated user.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetAllTransactionQuery query,
        IQueryHandler<GetAllTransactionQuery, Result<PagedResult<TransactionDetailResponse>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetTransactionQuery, Result<TransactionDetailResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetTransactionQuery(id), cancellationToken);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.ToProblemDetails();
    }

    private static async Task<IResult> Create(
        CreateTransactionCommand command,
        ICommandHandler<CreateTransactionCommand, Result<CreateTransactionResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetTransactionById", new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateTransactionCommand request,
        ICommandHandler<UpdateTransactionCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await handler.HandleAsync(command, cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteTransactionCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteTransactionCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToProblemDetails();
    }
}
