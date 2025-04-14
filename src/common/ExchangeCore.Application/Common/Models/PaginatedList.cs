namespace ExchangeCore.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageIndex { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    //public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken)
    //{
    //    var count = await source.CountAsync(cancellationToken);
    //    var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    //    return new PaginatedList<T>(items, count, pageIndex, pageSize);
    //}

    // CreateAsync method for IDictionary<TKey, TValue>
    public static async Task<PaginatedList<KeyValuePair<TKey, TValue>>> CreateAsync<TKey, TValue>(
        IDictionary<TKey, TValue> source, int pageIndex, int pageSize)
    {
        var totalCount = source.Count;
        var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedList<KeyValuePair<TKey, TValue>>(
            items, totalCount, pageIndex, pageSize);
    }
}
