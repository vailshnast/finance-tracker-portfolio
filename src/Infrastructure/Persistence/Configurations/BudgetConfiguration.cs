using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
            .IsUnique();

        builder.Property(b => b.Limit).IsRequired();
        builder.Property(b => b.Month).IsRequired();
        builder.Property(b => b.Year).IsRequired();

        builder.HasOne(b => b.User)
            .WithMany(t => t.Budgets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
