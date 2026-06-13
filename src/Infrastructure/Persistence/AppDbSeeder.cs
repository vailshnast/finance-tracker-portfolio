using FinanceTracker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Persistence;

public static class AppDbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager, logger);
        await SeedAdminUserAsync(userManager, logger);
        await SeedTestDataAsync(context, userManager, logger);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string adminEmail = "admin@gmail.com";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            FirstName = "Admin",
            LastName = "User",
            Email = adminEmail,
            UserName = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
        //log errors
        else logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private static async Task SeedTestDataAsync(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var admin = await userManager.FindByEmailAsync("admin@gmail.com");
        if (admin is null) return;

        if (await context.Categories.AnyAsync(c => c.UserId == admin.Id)) return;

        var salary      = new Category { Id = Guid.NewGuid(), Name = "Salary",        Type = CategoryType.Income,  UserId = admin.Id, CreatedAt = DateTimeOffset.UtcNow };
        var freelance   = new Category { Id = Guid.NewGuid(), Name = "Freelance",      Type = CategoryType.Income,  UserId = admin.Id, CreatedAt = DateTimeOffset.UtcNow };
        var groceries   = new Category { Id = Guid.NewGuid(), Name = "Groceries",      Type = CategoryType.Expense, UserId = admin.Id, CreatedAt = DateTimeOffset.UtcNow };
        var rent        = new Category { Id = Guid.NewGuid(), Name = "Rent",           Type = CategoryType.Expense, UserId = admin.Id, CreatedAt = DateTimeOffset.UtcNow };
        var entertainment = new Category { Id = Guid.NewGuid(), Name = "Entertainment", Type = CategoryType.Expense, UserId = admin.Id, CreatedAt = DateTimeOffset.UtcNow };

        context.Categories.AddRange(salary, freelance, groceries, rent, entertainment);

        context.Transactions.AddRange(
            // Salary — June only
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1),  Amount = 3000m, Description = "Monthly salary",         CategoryId = salary.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 5, 1),  Amount = 3000m, Description = "Monthly salary",         CategoryId = salary.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 4, 1),  Amount = 3200m, Description = "Monthly salary + bonus", CategoryId = salary.Id, UserId = admin.Id },

            // Freelance — June only
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 3),  Amount = 500m,  Description = "Freelance project A",    CategoryId = freelance.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 8),  Amount = 750m,  Description = "Freelance project B",    CategoryId = freelance.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 14), Amount = 300m,  Description = "Freelance consultation", CategoryId = freelance.Id, UserId = admin.Id },

            // Groceries — June only
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 2),  Amount = 150m,  Description = "Weekly groceries",       CategoryId = groceries.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 9),  Amount = 200m,  Description = "Grocery top-up",         CategoryId = groceries.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 16), Amount = 95m,   Description = "Farmers market",         CategoryId = groceries.Id, UserId = admin.Id },

            // Rent — spread across months to test filtering
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1),  Amount = 1200m, Description = "June rent",              CategoryId = rent.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 5, 1),  Amount = 1200m, Description = "May rent",               CategoryId = rent.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 4, 1),  Amount = 1200m, Description = "April rent",             CategoryId = rent.Id, UserId = admin.Id },

            // Entertainment — June only
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 7),  Amount = 80m,   Description = "Cinema tickets",         CategoryId = entertainment.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 1),  Amount = 45m,   Description = "Streaming subscriptions", CategoryId = entertainment.Id, UserId = admin.Id },
            new Transaction { Id = Guid.NewGuid(), Date = new DateOnly(2026, 6, 11), Amount = 120m,  Description = "Concert tickets",        CategoryId = entertainment.Id, UserId = admin.Id }
        );

        context.Budgets.Add(new Budget
        {
            Id = Guid.NewGuid(),
            Limit = 400m,
            Month = 6,
            Year = 2026,
            CategoryId = groceries.Id,
            UserId = admin.Id,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
        logger.LogInformation("Seeded test categories, transactions, and budget for {Email}", admin.Email);
    }
}
