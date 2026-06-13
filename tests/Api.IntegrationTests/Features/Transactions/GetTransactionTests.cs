using FinanceTracker.Application.Features.Transactions.Get;
using FinanceTracker.Application.Features.Transactions.GetAll;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.IntegrationTests.Features.Transactions;

[Collection(IntegrationCollection.Name)]
public sealed class GetTransactionTests(ApiFactory factory)
{
    [Fact]
    public async Task GetTransaction_WithValidId_ReturnsTransaction()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);
        var transactionId = await factory.SeedTransactionAsync(userId, categoryId, 75m, "Lunch", ct);

        var response = await client.GetAsync($"/api/transaction/{transactionId}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var transaction = await response.Content.ReadFromJsonAsync<TransactionDetailResponse>(ct);
        transaction!.Id.Should().Be(transactionId);
        transaction.Amount.Should().Be(75m);
        transaction.Description.Should().Be("Lunch");
    }

    [Fact]
    public async Task GetTransaction_WithUnknownId_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        var response = await client.GetAsync($"/api/transaction/{Guid.NewGuid()}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransaction_BelongingToAnotherUser_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, _) = await factory.CreateAuthenticatedClientAsync(ct);
        var (_, userIdB) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userIdB, cancellationToken: ct);
        var transactionId = await factory.SeedTransactionAsync(userIdB, categoryId, cancellationToken: ct);

        // handler filters by userId, so 404 not 403
        var response = await clientA.GetAsync($"/api/transaction/{transactionId}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllTransactions_ReturnsPaginatedListForCurrentUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);

        await factory.SeedTransactionAsync(userId, categoryId, 10m, "Coffee", ct);
        await factory.SeedTransactionAsync(userId, categoryId, 20m, "Lunch", ct);
        await factory.SeedTransactionAsync(userId, categoryId, 30m, "Dinner", ct);

        var response = await client.GetAsync("/api/transaction?page=1&pageSize=10", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDetailResponse>>(ct);
        page!.Items.Should().HaveCount(3);
        page.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllTransactions_Pagination_ReturnsCorrectPage()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, cancellationToken: ct);

        for (var i = 1; i <= 5; i++)
            await factory.SeedTransactionAsync(userId, categoryId, i * 10m, $"Transaction {i}", ct);

        var response = await client.GetAsync("/api/transaction?page=2&pageSize=2", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDetailResponse>>(ct);
        page!.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllTransactions_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/transaction", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}