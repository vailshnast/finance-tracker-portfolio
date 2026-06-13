using FinanceTracker.Application.Features.Budgets.Create;

namespace FinanceTracker.Api.IntegrationTests.Features.Budgets;

[Collection(IntegrationCollection.Name)]
public sealed class CreateBudgetTests(ApiFactory factory)
{
    [Fact]
    public async Task CreateBudget_WithValidData_Returns201WithLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Food", cancellationToken: ct);
        var command = new CreateBudgetCommand(Limit: 500m, Month: 6, Year: 2026, CategoryId: categoryId);

        var response = await client.PostAsJsonAsync("/api/budgets", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreateBudgetResponse>(ct);
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBudget_WithNegativeLimit_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);
        var command = new CreateBudgetCommand(Limit: -100m, Month: 6, Year: 2026, CategoryId: categoryId);

        var response = await client.PostAsJsonAsync("/api/budgets", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Limit");
    }

    [Fact]
    public async Task CreateBudget_WithInvalidMonth_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);
        var command = new CreateBudgetCommand(Limit: 300m, Month: 13, Year: 2026, CategoryId: categoryId);

        var response = await client.PostAsJsonAsync("/api/budgets", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Month");
    }

    [Fact]
    public async Task CreateBudget_DuplicateCategoryMonthYear_ReturnsConflict()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);
        var command = new CreateBudgetCommand(Limit: 300m, Month: 1, Year: 2026, CategoryId: categoryId);

        await client.PostAsJsonAsync("/api/budgets", command, ct);

        // Act â€” second request with the same category + month + year
        var response = await client.PostAsJsonAsync("/api/budgets", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateBudget_WithoutToken_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/budgets",
                new CreateBudgetCommand(300m, 6, 2026, Guid.NewGuid()), ct);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
