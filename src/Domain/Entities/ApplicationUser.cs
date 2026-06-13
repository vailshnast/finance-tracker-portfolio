namespace FinanceTracker.Domain.Entities;

using Microsoft.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
