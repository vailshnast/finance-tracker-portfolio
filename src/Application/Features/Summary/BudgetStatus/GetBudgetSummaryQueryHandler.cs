using FinanceTracker.Application.Abstractions.Data;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Abstractions.Messaging;
using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Application.Features.Summary.BudgetStatus;

public sealed class GetBudgetSummaryQueryHandler(ICurrentUser currentUser, IAppDbContext db) : IQueryHandler<GetBudgetSummaryQuery, Result<GetBudgetSummaryResponse>>
{
    public async Task<Result<GetBudgetSummaryResponse>> HandleAsync(GetBudgetSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var userId = currentUser.UserId!;

        var budgets = await db.Budgets
            .Where(b => b.Year == year && b.Month == month && b.UserId == userId)
            .Select(b => new { b.CategoryId, CategoryName = b.Category.Name, b.Limit })
            .ToListAsync(cancellationToken);

        var categoryIds = budgets.Select(b => b.CategoryId).ToList();

        var spentByCategory = await db.Transactions
            .InPeriodForUser(userId, year, month, categoryIds)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Spent = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Spent, cancellationToken);

        var summaries = budgets
            .Select(b => new BudgetSummary
            {
                CategoryId = b.CategoryId,
                CategoryName = b.CategoryName,
                Limit = b.Limit,
                Spent = spentByCategory.GetValueOrDefault(b.CategoryId, 0)
            })
            .ToList();

        return Result.Success(new GetBudgetSummaryResponse
        {
            Year = year,
            Month = month,
            Budgets = summaries
        });
    }
}

public static class TransactionQueryExtensions
{
    public static IQueryable<Transaction> InPeriodForUser(this IQueryable<Transaction> transactions,
        string userId, int year, int month, List<Guid> categoryIds)
    {
        return transactions.Where(t =>t.UserId == userId &&
                                      t.Date.Year == year &&
                                      t.Date.Month == month &&
                                      categoryIds.Contains(t.CategoryId));
    }
}
