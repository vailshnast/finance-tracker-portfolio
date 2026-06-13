using FinanceTracker.Application.Features.Summary.BudgetStatus;

namespace FinanceTracker.Api.IntegrationTests.Features.Summary;

[Collection(IntegrationCollection.Name)]
public sealed class GetBudgetSummaryTests(ApiFactory factory)
{
    private static readonly int Year = DateTime.UtcNow.Year;
    private static readonly int Month = DateTime.UtcNow.Month;

    [Fact]
    public async Task GetBudgetSummary_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/summary/budgets", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBudgetSummary_WithNoBudgets_ReturnsEmptyList()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        var response = await client.GetAsync("/api/summary/budgets", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetBudgetSummaryResponse>(ct);
        summary!.Budgets.Should().BeEmpty();
        summary.Year.Should().Be(Year);
        summary.Month.Should().Be(Month);
    }

    [Fact]
    public async Task GetBudgetSummary_WithBudgetAndNoSpending_ReturnsZeroSpent()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Food", CategoryType.Expense, ct);
        await factory.SeedBudgetAsync(userId, categoryId, 500m, Month, Year, ct);

        var response = await client.GetAsync("/api/summary/budgets", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetBudgetSummaryResponse>(ct);
        summary!.Budgets.Should().HaveCount(1);
        var budget = summary.Budgets[0];
        budget.CategoryName.Should().Be("Food");
        budget.Limit.Should().Be(500m);
        budget.Spent.Should().Be(0m);
        budget.Remaining.Should().Be(500m);
        budget.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetBudgetSummary_WithSpending_CalculatesRemainingCorrectly()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Transport", CategoryType.Expense, ct);
        await factory.SeedBudgetAsync(userId, categoryId, 300m, Month, Year, ct);
        var date = new DateOnly(Year, Month, 1);
        await factory.SeedTransactionAsync(userId, categoryId, 80m, null, date, ct);
        await factory.SeedTransactionAsync(userId, categoryId, 70m, null, date, ct);

        var response = await client.GetAsync("/api/summary/budgets", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetBudgetSummaryResponse>(ct);
        var budget = summary!.Budgets.Should().ContainSingle().Subject;
        budget.Spent.Should().Be(150m);
        budget.Remaining.Should().Be(150m);
        budget.IsExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task GetBudgetSummary_WhenSpendingExceedsLimit_MarksIsExceededTrue()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Entertainment", CategoryType.Expense, ct);
        await factory.SeedBudgetAsync(userId, categoryId, 100m, Month, Year, ct);
        var date = new DateOnly(Year, Month, 1);
        await factory.SeedTransactionAsync(userId, categoryId, 150m, null, date, ct);

        var response = await client.GetAsync("/api/summary/budgets", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetBudgetSummaryResponse>(ct);
        var budget = summary!.Budgets.Should().ContainSingle().Subject;
        budget.Spent.Should().Be(150m);
        budget.IsExceeded.Should().BeTrue();
        budget.Remaining.Should().Be(-50m);
    }

    [Fact]
    public async Task GetBudgetSummary_OnlyReturnsCurrentUserBudgets()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, userIdA) = await factory.CreateAuthenticatedClientAsync(ct);
        var (_, userIdB) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryIdA = await factory.SeedCategoryAsync(userIdA, "Food", cancellationToken: ct);
        var categoryIdB = await factory.SeedCategoryAsync(userIdB, "Food", cancellationToken: ct);
        await factory.SeedBudgetAsync(userIdA, categoryIdA, 500m, Month, Year, ct);
        await factory.SeedBudgetAsync(userIdB, categoryIdB, 300m, Month, Year, ct);

        var response = await clientA.GetAsync("/api/summary/budgets", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetBudgetSummaryResponse>(ct);
        summary!.Budgets.Should().HaveCount(1);
        summary.Budgets[0].Limit.Should().Be(500m);
    }
}
