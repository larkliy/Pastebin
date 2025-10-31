namespace Pastebin.DTOs.Shared;

public class PaginatedResponse<T>(IEnumerable<T> items, int pageNumber, int pageSize, bool hasNextPage)
{
    public IEnumerable<T> Items { get; } = items;
    public int PageNumber { get; } = pageNumber;
    public int PageSize { get; } = pageSize;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage { get; } = hasNextPage;
}
