using ExchangeCore.Application.Common.Models;

namespace ExchangeCore.Application.Common.Mapping;

public static class MappingExtensions
{
    public static Task<PaginatedList<KeyValuePair<TKey, TValue>>> PaginatedListAsync<TKey, TValue>(
    this IDictionary<TKey, TValue> dictionary, int pageNumber, int pageSize)
    {
        var totalCount = dictionary.Count;
        var items = dictionary.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(new PaginatedList<KeyValuePair<TKey, TValue>>(
            items, totalCount, pageNumber, pageSize));
    }
}
