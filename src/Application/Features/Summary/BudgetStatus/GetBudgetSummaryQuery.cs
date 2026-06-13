using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Application.Features.Summary.BudgetStatus;

public sealed record GetBudgetSummaryQuery() : IQuery<Result<GetBudgetSummaryResponse>>;
public sealed record GetBudgetSummaryResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public List<BudgetSummary> Budgets { get; init; } = new();
}

public sealed record BudgetSummary
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Limit - Spent;
    public decimal PercentageUsed => Spent / Limit  * 100;
    public bool IsExceeded => Spent > Limit;
}
