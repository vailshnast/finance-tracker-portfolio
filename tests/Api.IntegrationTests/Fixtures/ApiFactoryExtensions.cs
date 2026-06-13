using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Aspire.Hosting.Testing;
using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Identity.Login;
using FinanceTracker.Application.Features.Identity.Register;
using FinanceTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.IntegrationTests.Fixtures;

public static class ApiFactoryExtensions
{
    // Creates a unique user via the real register/login endpoints and returns an authenticated
    // HttpClient with the Bearer token set, plus the userId extracted from the JWT claims.
    public static async Task<(HttpClient Client, string UserId)> CreateAuthenticatedClientAsync(
        this ApiFactory factory,
        CancellationToken cancellationToken = default)
    {
        var client = factory.CreateClient();
        var email = $"test-{Guid.NewGuid()}@example.com";
        const string password = "Password1!";

        await client.PostAsJsonAsync("/api/identity/register",
            new RegisterCommand("Test", "User", email, password, password), cancellationToken);

        var loginResponse = await client.PostAsJsonAsync("/api/identity/login",
            new LoginCommand(email, password), cancellationToken);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens!.AccessToken);
        var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return (client, userId);
    }

    public static async Task<Guid> SeedCategoryAsync(
        this ApiFactory factory,
        string userId,
        string name = "Test Category",
        CategoryType type = CategoryType.Expense,
        CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var category = new Category { Name = name, Type = type, UserId = userId };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public static async Task<Guid> SeedTransactionAsync(
        this ApiFactory factory,
        string userId,
        Guid categoryId,
        decimal amount = 100m,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var transaction = new Transaction
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Amount = amount,
            Description = description,
            CategoryId = categoryId,
            UserId = userId
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return transaction.Id;
    }

    public static async Task<Guid> SeedTransactionAsync(
        this ApiFactory factory,
        string userId,
        Guid categoryId,
        decimal amount,
        string? description,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var transaction = new Transaction
        {
            Date = date,
            Amount = amount,
            Description = description,
            CategoryId = categoryId,
            UserId = userId
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        return transaction.Id;
    }

    public static async Task<Guid> SeedBudgetAsync(
        this ApiFactory factory,
        string userId,
        Guid categoryId,
        decimal limit,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);

        var budget = new Budget { UserId = userId, CategoryId = categoryId, Limit = limit, Month = month, Year = year };
        db.Budgets.Add(budget);
        await db.SaveChangesAsync(cancellationToken);
        return budget.Id;
    }

    private static async Task<AppDbContext> CreateDbContextAsync(
        this ApiFactory factory, CancellationToken cancellationToken)
    {
        var connectionString = await factory.App.GetConnectionStringAsync("db", cancellationToken);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AppDbContext(options);
    }
}
