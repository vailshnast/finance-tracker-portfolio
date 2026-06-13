namespace FinanceTracker.Application.Features.Budgets.Create;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record CreateBudgetCommand(decimal Limit, int Month, int Year, Guid CategoryId) : ICommand<Result<CreateBudgetResponse>>;

public sealed record CreateBudgetResponse(Guid Id);
