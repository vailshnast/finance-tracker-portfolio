namespace FinanceTracker.Application.Features.Transactions.Get;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record GetTransactionQuery(Guid Id) : IQuery<Result<TransactionDetailResponse>>;

public sealed record TransactionDetailResponse(Guid Id, decimal Amount, string? Description, Guid CategoryId, DateTimeOffset CreatedAt);
