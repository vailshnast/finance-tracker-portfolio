using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Application.Features.Summary.Monthly;

public sealed record GetMonthlySummaryQuery(int Month, int Year) : IQuery<Result<GetMonthlySummaryResponse>>;
public sealed record GetMonthlySummaryResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance => TotalIncome - TotalExpenses;
    public List<CategorySummary> CategorySummaries { get; init; } = new();
}

public sealed record CategorySummary
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public CategoryType CategoryType { get; set; }
    public decimal Total { get; set; }
    public decimal TransactionAmount { get; set; }
}
