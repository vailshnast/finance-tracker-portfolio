namespace FinanceTracker.Application.Features.Budgets.Update;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record UpdateBudgetCommand(Guid Id, decimal Limit, int Month, int Year, Guid CategoryId) : ICommand;
