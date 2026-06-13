using FinanceTracker.Application.Features.Transactions.Create;

namespace FinanceTracker.Api.IntegrationTests.Features.Transactions;

[Collection(IntegrationCollection.Name)]
public sealed class CreateTransactionTests(ApiFactory factory)
{
    [Fact]
    public async Task CreateTransaction_WithValidData_Returns201WithLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Groceries", cancellationToken: ct);
        var command = new CreateTransactionCommand(
            Date: new DateOnly(2026, 6, 1),
            Amount: 49.99m,
            Description: "Weekly shop",
            CategoryId: categoryId);

        var response = await client.PostAsJsonAsync("/api/transaction", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreateTransactionResponse>(ct);
        body!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateTransaction_WithNegativeAmount_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);
        var command = new CreateTransactionCommand(
            Date: new DateOnly(2026, 6, 1),
            Amount: -50m,
            Description: null,
            CategoryId: categoryId);

        var response = await client.PostAsJsonAsync("/api/transaction", command, ct);

        // CreateTransactionValidator: Amount > 0
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Amount");
    }

    [Fact]
    public async Task CreateTransaction_WithEmptyCategoryId_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);
        var command = new CreateTransactionCommand(
            Date: new DateOnly(2026, 6, 1),
            Amount: 50m,
            Description: null,
            CategoryId: Guid.Empty);

        var response = await client.PostAsJsonAsync("/api/transaction", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("CategoryId");
    }

    [Fact]
    public async Task CreateTransaction_WithoutToken_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/transaction",
                new CreateTransactionCommand(new DateOnly(2026, 1, 1), 50m, null, Guid.NewGuid()), ct);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
