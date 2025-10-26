namespace Pastebin.DTOs.Shared;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage { get; }

    public PaginatedResponse(IEnumerable<T> items, int pageNumber, int pageSize, bool hasNextPage)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        HasNextPage = hasNextPage;
    }
}
