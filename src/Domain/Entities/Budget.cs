using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Entities;

public sealed class Budget : AuditableEntity
{
    public decimal Limit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid CategoryId { get; set; }
    public string UserId { get; init; } = null!;

    public ApplicationUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
