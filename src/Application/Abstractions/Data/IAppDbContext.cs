namespace FinanceTracker.Application.Abstractions.Data;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Budget> Budgets { get; }
    DbSet<ApplicationUser> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
