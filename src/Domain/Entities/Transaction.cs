using FinanceTracker.Domain.Common;

namespace FinanceTracker.Domain.Entities;

public sealed class Transaction : AuditableEntity
{
    public DateOnly Date { get; set; }
    public decimal Amount  { get; set; }
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
