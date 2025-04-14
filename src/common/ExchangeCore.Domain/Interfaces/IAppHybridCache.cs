using Microsoft.Extensions.Caching.Hybrid;

namespace ExchangeCore.Domain.Interfaces;

public interface IAppHybridCache
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken = default);
}
