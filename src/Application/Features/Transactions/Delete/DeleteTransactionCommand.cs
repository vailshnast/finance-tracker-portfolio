namespace FinanceTracker.Application.Features.Transactions.Delete;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record DeleteTransactionCommand(Guid Id) : ICommand;
