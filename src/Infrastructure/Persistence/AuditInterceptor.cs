using System.Security.Claims;
using FinanceTracker.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FinanceTracker.Infrastructure.Persistence;

// Singleton: IHttpContextAccessor is singleton and accesses AsyncLocal HttpContext per-call,
// so no scoped-from-root DI violation with AddDbContextPool.
public sealed class AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    : SaveChangesInterceptor, IDbContextOptionsConfiguration<AppDbContext>
{
    private string? CurrentUserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        entry.Entity.CreatedBy = CurrentUserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTimeOffset.UtcNow;
                        entry.Entity.LastModifiedBy = CurrentUserId;
                        break;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    void IDbContextOptionsConfiguration<AppDbContext>.Configure(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.AddInterceptors(this);
}
