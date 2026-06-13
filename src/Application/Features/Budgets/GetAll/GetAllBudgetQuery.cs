namespace FinanceTracker.Application.Features.Budgets.GetAll;

using Application.Abstractions.Messaging;
using Application.Features.Budgets.Get;
using Domain.Common;

public sealed record GetAllBudgetQuery(int Page = 1, int PageSize = 10) : IQuery<Result<PagedResult<BudgetDetailResponse>>>;
