using ExchangeCore.Domain.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;

namespace ExchangeCore.Infrastructure.Services;

public class AppHybridCache : IAppHybridCache
{
    private readonly HybridCache _hybridCache;

    public AppHybridCache(HybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        return await _hybridCache.GetOrCreateAsync(key: key, factory: factory, options: options, cancellationToken: cancellationToken);
    }
}
