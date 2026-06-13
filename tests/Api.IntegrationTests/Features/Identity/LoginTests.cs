using FinanceTracker.Application.Abstractions.Identity;
using FinanceTracker.Application.Features.Identity.Login;
using FinanceTracker.Application.Features.Identity.Register;

namespace FinanceTracker.Api.IntegrationTests.Features.Identity;

[Collection(IntegrationCollection.Name)]
public sealed class LoginTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"{Guid.NewGuid()}@example.com";
        var password = "Password1!";

        await _client.PostAsJsonAsync("/api/identity/register",
            new RegisterCommand("John", "Test", email, password, password), ct);

        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new LoginCommand(email, password), ct);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        tokens!.AccessToken.Should().NotBeNullOrEmpty();
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/identity/register",
            new RegisterCommand("John", "Test", email, "Password1!", "Password1!"), ct);

        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new LoginCommand(email, "WrongPassword99!"), ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new LoginCommand("nobody@example.com", "Password1!"), ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsValidationProblem()
    {
        var ct = TestContext.Current.CancellationToken;
        var response = await _client.PostAsJsonAsync("/api/identity/login",
            new LoginCommand("test@example.com", ""), ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(ct);
        problem!.Errors.Should().ContainKey("Password");
    }
}
