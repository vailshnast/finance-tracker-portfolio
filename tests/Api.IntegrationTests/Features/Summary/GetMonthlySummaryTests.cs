using FinanceTracker.Application.Features.Summary.Monthly;

namespace FinanceTracker.Api.IntegrationTests.Features.Summary;

[Collection(IntegrationCollection.Name)]
public sealed class GetMonthlySummaryTests(ApiFactory factory)
{
    private static readonly int Year = DateTime.UtcNow.Year;
    private static readonly int Month = DateTime.UtcNow.Month;

    [Fact]
    public async Task GetMonthlySummary_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/summary/monthly", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMonthlySummary_WithNoTransactions_ReturnsEmptyResponse()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        var response = await client.GetAsync($"/api/summary/monthly?month={Month}&year={Year}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetMonthlySummaryResponse>(ct);
        summary!.CategorySummaries.Should().BeEmpty();
        summary.TotalIncome.Should().Be(0);
        summary.TotalExpenses.Should().Be(0);
        summary.NetBalance.Should().Be(0);
    }

    [Fact]
    public async Task GetMonthlySummary_WithTransactions_ReturnsCorrectTotals()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Groceries", CategoryType.Expense, ct);
        var date = new DateOnly(Year, Month, 1);

        await factory.SeedTransactionAsync(userId, categoryId, 60m, "Shop A", date, ct);
        await factory.SeedTransactionAsync(userId, categoryId, 40m, "Shop B", date, ct);

        var response = await client.GetAsync($"/api/summary/monthly?month={Month}&year={Year}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetMonthlySummaryResponse>(ct);
        summary!.TotalExpenses.Should().Be(100m);
        summary.TotalIncome.Should().Be(0);
        summary.NetBalance.Should().Be(-100m);
        summary.CategorySummaries.Should().HaveCount(1);
        summary.CategorySummaries[0].CategoryName.Should().Be("Groceries");
        summary.CategorySummaries[0].Total.Should().Be(100m);
    }

    [Fact]
    public async Task GetMonthlySummary_WithIncomeAndExpense_CalculatesNetBalance()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var incomeId = await factory.SeedCategoryAsync(userId, "Salary", CategoryType.Income, ct);
        var expenseId = await factory.SeedCategoryAsync(userId, "Rent", CategoryType.Expense, ct);
        var date = new DateOnly(Year, Month, 1);

        await factory.SeedTransactionAsync(userId, incomeId, 4000m, null, date, ct);
        await factory.SeedTransactionAsync(userId, expenseId, 1500m, null, date, ct);

        var response = await client.GetAsync($"/api/summary/monthly?month={Month}&year={Year}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetMonthlySummaryResponse>(ct);
        summary!.TotalIncome.Should().Be(4000m);
        summary.TotalExpenses.Should().Be(1500m);
        summary.NetBalance.Should().Be(2500m);
    }

    [Fact]
    public async Task GetMonthlySummary_WithSpecificMonthYear_FiltersCorrectly()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);

        await factory.SeedTransactionAsync(userId, categoryId, 100m, null, new DateOnly(2025, 6, 1), ct);
        await factory.SeedTransactionAsync(userId, categoryId, 200m, null, new DateOnly(2025, 7, 1), ct);

        var response = await client.GetAsync("/api/summary/monthly?month=6&year=2025", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetMonthlySummaryResponse>(ct);
        summary!.TotalExpenses.Should().Be(100m);
        summary.Month.Should().Be(6);
        summary.Year.Should().Be(2025);
    }

    [Fact]
    public async Task GetMonthlySummary_DefaultsToCurrentMonthYear()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        var response = await client.GetAsync("/api/summary/monthly", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<GetMonthlySummaryResponse>(ct);
        summary!.Month.Should().Be(Month);
        summary.Year.Should().Be(Year);
    }
}
