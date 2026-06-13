namespace FinanceTracker.Application.Features.Budgets.Delete;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record DeleteBudgetCommand(Guid Id) : ICommand;
