namespace FinanceTracker.Application.Features.Budgets.Get;

using Application.Abstractions.Messaging;
using Domain.Common;

public sealed record GetBudgetQuery(Guid Id) : IQuery<Result<BudgetDetailResponse>>;

public sealed record BudgetDetailResponse(Guid Id, decimal Limit, int Month, int Year, Guid CategoryId);
