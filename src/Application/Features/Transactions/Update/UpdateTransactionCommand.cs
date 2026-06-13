namespace FinanceTracker.Application.Features.Transactions.Update;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record UpdateTransactionCommand(Guid Id, DateOnly Date, decimal Amount, string? Description, Guid CategoryId) : ICommand;
