namespace FinanceTracker.Api.IntegrationTests.Fixtures;

// Minimal representation of the ASP.NET Core ValidationProblemDetails JSON shape.
// Used instead of HttpValidationProblemDetails to avoid requiring the ASP.NET Core framework reference.
public sealed record ValidationProblem(Dictionary<string, string[]>? Errors);
