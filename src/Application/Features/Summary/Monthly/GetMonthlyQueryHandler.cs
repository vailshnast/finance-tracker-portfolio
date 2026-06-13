using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Features.Summary.Monthly;

public sealed class GetMonthlyQueryHandler(IAppDbContext db, ICurrentUser currentUser) : IQueryHandler<GetMonthlySummaryQuery,Result<GetMonthlySummaryResponse>>
{
    public async Task<Result<GetMonthlySummaryResponse>> HandleAsync(GetMonthlySummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var categorySummaryList = await db.Transactions
            .Where(t => t.UserId == currentUser.UserId && t.Date.Year == query.Year && t.Date.Month == query.Month)
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Type })
            .Select(categoryTransactions => new CategorySummary
            {
                CategoryId = categoryTransactions.Key.CategoryId,
                CategoryName = categoryTransactions.Key.Name,
                CategoryType = categoryTransactions.Key.Type,
                Total = categoryTransactions.Sum(t => t.Amount),
                TransactionAmount = categoryTransactions.Count()
            })
            .ToListAsync(cancellationToken);


        var summaryResponse = new GetMonthlySummaryResponse
        {
            CategorySummaries = categorySummaryList,
            Month = query.Month,
            Year = query.Year,
            TotalIncome = categorySummaryList.TotalByType(CategoryType.Income),
            TotalExpenses = categorySummaryList.TotalByType(CategoryType.Expense),
        };


        return Result.Success(summaryResponse);
    }

}

public static class CategorySummaryExtensions
{
    public static decimal TotalByType(this IEnumerable<CategorySummary> categories, CategoryType type) =>
        categories.Where(c => c.CategoryType == type).Sum(c => c.Total);
}
