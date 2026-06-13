namespace FinanceTracker.Application.Features.Transactions.Create;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record CreateTransactionCommand(DateOnly Date, decimal Amount, string? Description, Guid CategoryId) : ICommand<Result<CreateTransactionResponse>>;

public sealed record CreateTransactionResponse(Guid Id);
