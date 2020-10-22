namespace api.Helpers
{
  public class PaginationHeader
  {
    public PaginationHeader(int currentPage, int totalPages, int pageSize, int totalCount)
    {
      CurrentPage = currentPage;
      TotalPages = totalPages;
      PageSize = pageSize;
      TotalCount = totalCount;
    }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
  }
}
