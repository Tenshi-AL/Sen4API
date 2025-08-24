namespace Infrastructure.Models;


public class PaginatedList<T>
{
    public IEnumerable<T> List { get; set; }
    public int? PageIndex { get; }
    public int? TotalPages { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public PaginatedList(IEnumerable<T> list, int? pageIndex, int? totalPages)
    {
        List = list;
        PageIndex = pageIndex;
        TotalPages = totalPages;
    }
}