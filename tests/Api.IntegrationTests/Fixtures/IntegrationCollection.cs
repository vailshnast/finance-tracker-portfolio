namespace FinanceTracker.Api.IntegrationTests.Fixtures;

// One PostgreSQL container is shared across all integration test classes in this collection.
// Tests are isolated by creating a unique user per test, so they never see each other's data.
[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Integration";
}