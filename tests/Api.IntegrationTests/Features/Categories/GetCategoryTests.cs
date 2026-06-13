using FinanceTracker.Application.Features.Categories.Get;
using FinanceTracker.Domain.Common;

namespace FinanceTracker.Api.IntegrationTests.Features.Categories;

[Collection(IntegrationCollection.Name)]
public sealed class GetCategoryTests(ApiFactory factory)
{
    [Fact]
    public async Task GetAllCategories_ReturnsOnlyCurrentUserCategories()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, userIdA) = await factory.CreateAuthenticatedClientAsync(ct);
        var (clientB, userIdB) = await factory.CreateAuthenticatedClientAsync(ct);

        await factory.SeedCategoryAsync(userIdA, "User A Category", cancellationToken: ct);
        await factory.SeedCategoryAsync(userIdB, "User B Category", cancellationToken: ct);

        var response = await clientA.GetAsync("/api/categories", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<CategoryDetailResponse>>(ct);
        page!.Items.Should().OnlyContain(c => c.Name == "User A Category",
            "user A should not see user B's categories");
    }

    [Fact]
    public async Task GetCategory_WithValidId_ReturnsCategory()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, userId) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userId, "Salary", CategoryType.Income, ct);

        var response = await client.GetAsync($"/api/categories/{categoryId}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryDetailResponse>(ct);
        category!.Id.Should().Be(categoryId);
        category.Name.Should().Be("Salary");
        category.Type.Should().Be(CategoryType.Income);
    }

    [Fact]
    public async Task GetCategory_WithUnknownId_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        var response = await client.GetAsync($"/api/categories/{Guid.NewGuid()}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCategory_BelongingToAnotherUser_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        var (clientA, _) = await factory.CreateAuthenticatedClientAsync(ct);
        var (_, userIdB) = await factory.CreateAuthenticatedClientAsync(ct);
        var categoryId = await factory.SeedCategoryAsync(userIdB, "User B Private", cancellationToken: ct);

        // handler filters by userId, so it returns 404 rather than 403
        var response = await clientA.GetAsync($"/api/categories/{categoryId}", ct);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllCategories_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient()
            .GetAsync("/api/categories", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllCategories_PageSizeExceeds100_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);

        // GetAllCategoryQueryValidator enforces pageSize <= 100
        var response = await client.GetAsync("/api/categories?pageSize=999", ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
