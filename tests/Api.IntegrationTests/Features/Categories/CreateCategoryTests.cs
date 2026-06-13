using FinanceTracker.Application.Features.Categories.Create;

namespace FinanceTracker.Api.IntegrationTests.Features.Categories;

[Collection(IntegrationCollection.Name)]
public sealed class CreateCategoryTests(ApiFactory factory)
{
    [Fact]
    public async Task CreateCategory_WithValidData_Returns201WithLocation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);
        var command = new CreateCategoryCommand("Groceries", CategoryType.Expense);

        var response = await client.PostAsJsonAsync("/api/categories", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>(ct);
        body!.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Groceries");
        body.Type.Should().Be(CategoryType.Expense);
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, _) = await factory.CreateAuthenticatedClientAsync(ct);
        var command = new CreateCategoryCommand("", CategoryType.Expense);

        var response = await client.PostAsJsonAsync("/api/categories", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_Returns401()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new CreateCategoryCommand("Food", CategoryType.Expense);

        var response = await factory.CreateClient().PostAsJsonAsync("/api/categories", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
