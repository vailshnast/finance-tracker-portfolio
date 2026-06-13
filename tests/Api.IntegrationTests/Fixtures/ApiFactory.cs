using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Infrastructure.Persistence;

namespace FinanceTracker.Api.IntegrationTests.Fixtures;

public sealed class ApiFactory : IAsyncLifetime
{
    private DistributedApplication _app = null!;

    internal DistributedApplication App => _app;

    public async ValueTask InitializeAsync()
    {
        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FinanceTracker_AppHost>();

        _app = await builder.BuildAsync();
        await _app.StartAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        // Wait for Postgres to be healthy before running migrations.
        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("postgres", cts.Token);

        await ApplyMigrationsAsync(cts.Token);

        // Wait for the API to be healthy (it won't serve requests until this resolves).
        await _app.ResourceNotifications
            .WaitForResourceHealthyAsync("api", cts.Token);
    }

    public async ValueTask DisposeAsync() => await _app.DisposeAsync();

    public HttpClient CreateClient() => _app.CreateHttpClient("api");

    private async Task ApplyMigrationsAsync(CancellationToken cancellationToken)
    {
        var connectionString = await _app.GetConnectionStringAsync("db", cancellationToken);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using var db = new AppDbContext(options);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
