using Microsoft.Extensions.Caching.Hybrid;

namespace FinanceTracker.Application.UnitTests;

/// <summary>
/// HybridCache implementation for tests that always invokes the factory — no caching.
/// </summary>
internal sealed class PassthroughHybridCache : HybridCache
{
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
        => factory(state, cancellationToken);

    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
