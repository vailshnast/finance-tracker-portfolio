using FinanceTracker.Application.Features.Identity.Register;

namespace FinanceTracker.Api.IntegrationTests.Features.Identity;

[Collection(IntegrationCollection.Name)]
public sealed class RegisterTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_Returns200()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new RegisterCommand(
            FirstName: "Jane",
            LastName: "Doe",
            Email: $"{Guid.NewGuid()}@example.com",
            Password: "Password1!",
            ConfirmPassword: "Password1!");

        var response = await _client.PostAsJsonAsync("/api/identity/register", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new RegisterCommand(
            FirstName: "Jane",
            LastName: "Doe",
            Email: $"{Guid.NewGuid()}@example.com",
            Password: "Password1!",
            ConfirmPassword: "Different99!");

        var response = await _client.PostAsJsonAsync("/api/identity/register", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("ConfirmPassword");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var command = new RegisterCommand(
            FirstName: "Jane",
            LastName: "Doe",
            Email: "not-an-email",
            Password: "Password1!",
            ConfirmPassword: "Password1!");

        var response = await _client.PostAsJsonAsync("/api/identity/register", command, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"{Guid.NewGuid()}@example.com";
        var command = new RegisterCommand("Jane", "Doe", email, "Password1!", "Password1!");

        await _client.PostAsJsonAsync("/api/identity/register", command, ct);

        // Act â€” same email a second time
        var response = await _client.PostAsJsonAsync("/api/identity/register", command, ct);

        response.IsSuccessStatusCode.Should().BeFalse();
    }
}
