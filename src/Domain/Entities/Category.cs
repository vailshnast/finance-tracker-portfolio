using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Entities;

public enum CategoryType
{
    Income = 0,
    Expense = 1
}

public sealed class Category : AuditableEntity
{
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public bool IsDefault { get; set; }
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
