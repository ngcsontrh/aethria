namespace Aethria.Application.Models;

/// <summary>
/// Represents a paginated response containing a list of items and paging metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the page.</typeparam>
public sealed record PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; }

    public PagedResponse(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;
    }
}
